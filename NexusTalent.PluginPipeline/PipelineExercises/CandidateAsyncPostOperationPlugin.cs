using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginPipeline.PipelineExercises
{
    public class CandidateAsyncPostOperationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            var tracing = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            PluginTraceHelper.TraceContext(tracing, context);

            tracing.Trace("Asynchronous PostOperation plug-in started.");

            try
            {
                if (context.MessageName != "Create" ||
                    context.PrimaryEntityName != "nex_candidate")
                {
                    return;
                }

                tracing.Trace(
                    "Background processing completed for Candidate ID: {0}",
                    context.PrimaryEntityId);
            }
            catch (Exception ex)
            {
                tracing.Trace(
                    "Asynchronous processing failed: {0}",
                    ex.ToString());

                throw new InvalidPluginExecutionException(
                    "Asynchronous Candidate processing failed.", ex);
            }
        }
    }
}