using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace NexusTalent.PluginSet
{
    public class RequisitionInterviewRollupPlugin : IPlugin
    {
        private const string InterviewTable = "nex_interview";
        private const string InterviewId = "nex_interviewid";
        private const string RequisitionLookup = "nex_requisition";

        private const string RequisitionTable = "nex_requisition";
        private const string RequisitionId = "nex_requisitionid";
        private const string InterviewCount = "nex_interviewcount";

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            ITracingService tracing =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            tracing.Trace(
                "Interview Rollup started. Message={0}, Entity={1}, " +
                "Stage={2}, Mode={3}, Depth={4}, CorrelationId={5}",
                context.MessageName,
                context.PrimaryEntityName,
                context.Stage,
                context.Mode,
                context.Depth,
                context.CorrelationId);

            try
            {
                if (context.PrimaryEntityName != InterviewTable)
                {
                    tracing.Trace("Unexpected table. Exiting.");
                    return;
                }

                IOrganizationServiceFactory factory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(
                        typeof(IOrganizationServiceFactory));

                IOrganizationService service =
                    factory.CreateOrganizationService(context.UserId);

                Entity target = null;

                if (context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is Entity entity)
                {
                    target = entity;
                }

                EntityReference oldRequisition = null;
                EntityReference newRequisition = null;

                // On Delete, use the Pre-Image.
                if (context.MessageName == "Delete")
                {
                    if (context.PreEntityImages.Contains("PreImage"))
                    {
                        Entity preImage =
                            context.PreEntityImages["PreImage"];

                        oldRequisition =
                            preImage.GetAttributeValue<EntityReference>(
                                RequisitionLookup);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException(
                            "The Delete step requires a Pre-Image.");
                    }
                }

                // On Update, the Pre-Image provides the previous parent.
                if (context.MessageName == "Update")
                {
                    if (context.PreEntityImages.Contains("PreImage"))
                    {
                        Entity preImage =
                            context.PreEntityImages["PreImage"];

                        oldRequisition =
                            preImage.GetAttributeValue<EntityReference>(
                                RequisitionLookup);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException(
                            "The Update step requires a Pre-Image.");
                    }

                    if (target != null &&
                        target.Contains(RequisitionLookup))
                    {
                        newRequisition =
                            target.GetAttributeValue<EntityReference>(
                                RequisitionLookup);
                    }
                    else
                    {
                        newRequisition = oldRequisition;
                    }
                }

                // On Create, Target contains the new parent lookup.
                if (context.MessageName == "Create")
                {
                    if (target == null)
                    {
                        tracing.Trace("Create Target is missing.");
                        return;
                    }

                    newRequisition =
                        target.GetAttributeValue<EntityReference>(
                            RequisitionLookup);
                }

                tracing.Trace(
                    "Old Requisition={0}; New Requisition={1}",
                    oldRequisition?.Id,
                    newRequisition?.Id);

                // Recalculate the previous parent when the relationship
                // is removed or the Interview is moved.
                if (oldRequisition != null &&
                    (newRequisition == null ||
                     oldRequisition.Id != newRequisition.Id))
                {
                    RecalculateCount(
                        service,
                        oldRequisition.Id,
                        tracing);
                }

                // Recalculate the current parent.
                if (newRequisition != null)
                {
                    RecalculateCount(
                        service,
                        newRequisition.Id,
                        tracing);
                }

                tracing.Trace("Interview Rollup completed successfully.");
            }
            catch (Exception ex)
            {
                tracing.Trace(
                    "Interview Rollup failed: {0}", ex.ToString());

                throw new InvalidPluginExecutionException(
                    "Unable to update the Requisition Interview Count.");
            }
            finally
            {
                tracing.Trace(
                    "Interview Rollup finished. CorrelationId={0}",
                    context.CorrelationId);
            }
        }

        private static void RecalculateCount(
            IOrganizationService service,
            Guid requisitionId,
            ITracingService tracing)
        {
            tracing.Trace(
                "Recalculating Interview Count for Requisition {0}",
                requisitionId);

            int count = 0;
            int pageNumber = 1;
            string pagingCookie = null;
            bool moreRecords;

            do
            {
                QueryExpression query =
                    new QueryExpression(InterviewTable)
                    {
                        ColumnSet = new ColumnSet(InterviewId),
                        PageInfo = new PagingInfo
                        {
                            Count = 5000,
                            PageNumber = pageNumber,
                            PagingCookie = pagingCookie
                        }
                    };

                query.Criteria.AddCondition(
                    RequisitionLookup,
                    ConditionOperator.Equal,
                    requisitionId);

                EntityCollection interviews =
                    service.RetrieveMultiple(query);

                count += interviews.Entities.Count;
                moreRecords = interviews.MoreRecords;
                pagingCookie = interviews.PagingCookie;
                pageNumber++;
            }
            while (moreRecords);

            Entity update = new Entity(
                RequisitionTable,
                requisitionId);

            update[InterviewCount] = count;

            service.Update(update);

            tracing.Trace(
                "Requisition {0} updated. Interview Count={1}",
                requisitionId,
                count);
        }
    }
}