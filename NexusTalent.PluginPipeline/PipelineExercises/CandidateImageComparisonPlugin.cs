using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginPipeline.PipelineExercises
{
    public class CandidateImageComparisonPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            var tracing = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            PluginTraceHelper.TraceContext(tracing, context);

            if (context.MessageName != "Update" ||
                context.PrimaryEntityName != "nex_candidate")
            {
                return;
            }

            Entity target = context.InputParameters.Contains("Target")
                ? context.InputParameters["Target"] as Entity
                : null;

            if (target == null)
            {
                tracing.Trace("Target was not available.");
                return;
            }

            Entity preImage = context.PreEntityImages.Contains("PreImage")
                ? context.PreEntityImages["PreImage"]
                : null;

            Entity postImage = context.PostEntityImages.Contains("PostImage")
                ? context.PostEntityImages["PostImage"]
                : null;

            if (preImage == null || postImage == null)
            {
                tracing.Trace(
                    "PreImage or PostImage is missing. Check registration.");
                return;
            }

            int? oldStatus =
                preImage.Contains("nex_candidatestatus")
                ? preImage.GetAttributeValue<OptionSetValue>(
                    "nex_candidatestatus")?.Value
                : (int?)null;

            int? newStatus =
                postImage.Contains("nex_candidatestatus")
                ? postImage.GetAttributeValue<OptionSetValue>(
                    "nex_candidatestatus")?.Value
                : (int?)null;

            int? targetStatus =
                target.Contains("nex_candidatestatus")
                ? target.GetAttributeValue<OptionSetValue>(
                    "nex_candidatestatus")?.Value
                : (int?)null;

            tracing.Trace("Old status from PreImage: {0}",
                oldStatus.HasValue ? oldStatus.Value.ToString() : "<null>");

            tracing.Trace("Status supplied in Target: {0}",
                targetStatus.HasValue ? targetStatus.Value.ToString() : "<not supplied>");

            tracing.Trace("New status from PostImage: {0}",
                newStatus.HasValue ? newStatus.Value.ToString() : "<null>");

            if (oldStatus != newStatus)
            {
                tracing.Trace(
                    "Candidate status changed from {0} to {1}.",
                    oldStatus, newStatus);
            }
            else
            {
                tracing.Trace("Candidate status did not change.");
            }
        }
    }
}