using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginPipeline.PipelineExercises
{
    public class CandidatePostOperationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            var tracing = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            var serviceFactory = (IOrganizationServiceFactory)
                serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            var service = serviceFactory.CreateOrganizationService(
                context.UserId);

            PluginTraceHelper.TraceContext(tracing, context);

            if (context.MessageName != "Create" ||
                context.PrimaryEntityName != "nex_candidate")
            {
                return;
            }

            if (context.PrimaryEntityId == Guid.Empty)
            {
                throw new InvalidPluginExecutionException(
                    "Candidate ID was not available.");
            }

            try
            {
                var interview = new Entity("nex_interview");

                interview["nex_candidate"] =
                    new EntityReference(
                        "nex_candidate",
                        context.PrimaryEntityId);

                // Add any other required Interview columns here.
                // For example, set a required Subject or Interview Date
                // using the actual logical names and column types.

                Guid interviewId = service.Create(interview);

                tracing.Trace(
                    "Related Interview created successfully. ID: {0}",
                    interviewId);

                // Keep this false for the normal success test.
                bool simulateRollback = false;

                if (simulateRollback)
                {
                    throw new InvalidPluginExecutionException(
                        "Rollback test: deliberately cancelling Candidate creation.");
                }
            }
            catch (Exception ex)
            {
                tracing.Trace(
                    "PostOperation failed: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "The Candidate operation failed during related-record creation.",
                    ex);
            }
        }
    }
}