using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginPipeline.PipelineExercises
{
    public class CandidatePreOperationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            var tracing = (ITracingService)
                serviceProvider.GetService(typeof(ITracingService));

            PluginTraceHelper.TraceContext(tracing, context);

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

            if (!target.Contains("nex_plugintestscore"))
            {
                tracing.Trace("No test score supplied. Grade calculation skipped.");
                return;
            }

            int score = target.GetAttributeValue<int>("nex_plugintestscore");

            if (score < 0 || score > 100)
            {
                throw new InvalidPluginExecutionException(
                    "Test Score must be between 0 and 100.");
            }

            string grade;

            if (score >= 90)
                grade = "A";
            else if (score >= 75)
                grade = "B";
            else if (score >= 60)
                grade = "C";
            else
                grade = "D";

            target["nex_plugintestgrade"] = grade;

            tracing.Trace(
                "Calculated Test Grade. Score: {0}; Grade: {1}",
                score, grade);
        }
    }
}