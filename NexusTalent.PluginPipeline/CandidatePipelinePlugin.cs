using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginPipeline
{
    public class CandidatePipelinePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            var tracing =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            var serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            var service = serviceFactory.CreateOrganizationService(
                context.UserId);

            // Log execution details
            tracing.Trace(
                "Message={0}; Table={1}; Stage={2}; Mode={3}; " +
                "InTransaction={4}; Depth={5}",
                context.MessageName,
                context.PrimaryEntityName,
                context.Stage,
                context.Mode,
                context.IsInTransaction,
                context.Depth);

            // Only process Candidate records
            if (context.PrimaryEntityName != "nex_candidate")
                return;

            // 1. PreValidation: validate Candidate name
            if (context.MessageName == "Create" &&
                context.Stage == 10)
            {
                var target =
                    (Entity)context.InputParameters["Target"];

                string name =
                    target.GetAttributeValue<string>(
                        "nex_candidatename");

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new InvalidPluginExecutionException(
                        "Candidate name is required.");
                }

                tracing.Trace(
                    "PreValidation completed successfully.");
            }

            // 2. PreOperation: calculate the test grade
            if (context.MessageName == "Create" &&
                context.Stage == 20)
            {
                var target =
                    (Entity)context.InputParameters["Target"];

                if (target.Contains("nex_plugintestscore"))
                {
                    int score =
                        target.GetAttributeValue<int>(
                            "nex_plugintestscore");

                    target["nex_plugintestgrade"] =
                        score >= 70 ? "A" : "B";

                    tracing.Trace(
                        "Grade calculated from score: {0}", score);
                }
            }

            // 3. Synchronous PostOperation:
            // Create a related Interview after Candidate creation
            if (context.MessageName == "Create" &&
                context.Stage == 40 &&
                context.Mode == 0)
            {
                if (context.Depth > 1)
                {
                    tracing.Trace(
                        "Nested execution detected.");
                    return;
                }

                var candidateId =
                    (Guid)context.OutputParameters["id"];

                var interview = new Entity("nex_interview");

                interview["nex_candidate"] =
                    new EntityReference(
                        "nex_candidate", candidateId);

                // Add any other required Interview fields
                // before creating the record.

                service.Create(interview);

                tracing.Trace(
                    "Related Interview created for Candidate {0}.",
                    candidateId);

                // ROLLBACK TEST ONLY:
                // Uncomment the next line to test rollback.
                // Do not leave it enabled during normal use.
                //
                // throw new InvalidPluginExecutionException(
                //     "Rollback test: intentionally failing.");
            }

            // 4. Asynchronous PostOperation:
            // Log background execution
            if (context.MessageName == "Create" &&
                context.Stage == 40 &&
                context.Mode == 1)
            {
                tracing.Trace(
                    "Asynchronous Candidate processing completed.");
            }

            // 5. Update: compare PreImage, Target and PostImage
            if (context.MessageName == "Update" &&
                context.Stage == 40)
            {
                var target =
                    (Entity)context.InputParameters["Target"];

                Entity preImage = null;
                Entity postImage = null;

                if (context.PreEntityImages.Contains("PreImage"))
                    preImage =
                        context.PreEntityImages["PreImage"];

                if (context.PostEntityImages.Contains("PostImage"))
                    postImage =
                        context.PostEntityImages["PostImage"];

                int? oldStatus =
                    preImage?
                        .GetAttributeValue<OptionSetValue>(
                            "nex_candidatestatus")?.Value;

                int? requestedStatus =
                    target.Contains("nex_candidatestatus")
                    ? target.GetAttributeValue<OptionSetValue>(
                        "nex_candidatestatus")?.Value
                    : (int?)null;

                int? savedStatus =
                    postImage?
                        .GetAttributeValue<OptionSetValue>(
                            "nex_candidatestatus")?.Value;

                tracing.Trace(
                    "Status comparison: Old={0}; Target={1}; Saved={2}",
                    oldStatus, requestedStatus, savedStatus);
            }
        }
    }
}



