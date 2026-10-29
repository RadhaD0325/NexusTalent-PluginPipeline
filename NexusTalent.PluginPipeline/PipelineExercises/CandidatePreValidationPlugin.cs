using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginPipeline.PipelineExercises
{
    public class CandidatePreValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            var tracing = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            PluginTraceHelper.TraceContext(tracing, context);

            try
            {
                if (context.MessageName != "Create" ||
                    context.PrimaryEntityName != "nex_candidate")
                {
                    return;
                }

                if (!context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity target))
                {
                    return;
                }

                string candidateName =
                    target.GetAttributeValue<string>("nex_candidatename");

                if (string.IsNullOrWhiteSpace(candidateName))
                {
                    tracing.Trace("Validation failed: Candidate Name is required.");

                    throw new InvalidPluginExecutionException(
                        "Candidate Name is required. Please enter a Candidate Name.");
                }

                tracing.Trace("PreValidation passed.");
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracing.Trace("Unexpected error: {0}", ex.ToString());

                throw new InvalidPluginExecutionException(
                    "An error occurred during Candidate validation.", ex);
            }
        }
    }
}