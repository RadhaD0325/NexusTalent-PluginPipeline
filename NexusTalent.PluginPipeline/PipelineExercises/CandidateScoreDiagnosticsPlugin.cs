using System;
using Microsoft.Xrm.Sdk;

public class CandidateScoreDiagnosticsPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        ITracingService tracing =
            (ITracingService)serviceProvider.GetService(
                typeof(ITracingService));

        IPluginExecutionContext context =
            (IPluginExecutionContext)serviceProvider.GetService(
                typeof(IPluginExecutionContext));

        tracing.Trace(
            "START | Message={0} | Entity={1} | Stage={2} | Depth={3}",
            context.MessageName,
            context.PrimaryEntityName,
            context.Stage,
            context.Depth);

        if (context.MessageName != "Update" ||
            context.PrimaryEntityName != "nex_candidate")
        {
            tracing.Trace("DECISION | Unsupported message or entity");
            return;
        }

        if (!context.InputParameters.Contains("Target") ||
            !(context.InputParameters["Target"] is Entity target))
        {
            tracing.Trace("DECISION | Target record not available");
            return;
        }

        if (!target.Contains("nex_plugintestscore"))
        {
            tracing.Trace("DECISION | Score was not included in Target");
            return;
        }

        int score = target.GetAttributeValue<int>(
            "nex_plugintestscore");

        tracing.Trace("DECISION | Score={0}", score);

        if (score < 0 || score > 100)
        {
            tracing.Trace("VALIDATION | Invalid score");
            throw new InvalidPluginExecutionException(
                "Score must be between 0 and 100.");
        }

        tracing.Trace("VALIDATION | Score is valid");

        tracing.Trace(
            "END | CandidateId={0}",
            context.PrimaryEntityId);
    }
}

////Code	Purpose
//ITracingService	Writes trace messages.
//IPluginExecutionContext	Reads the current plug-in execution details.
//context.MessageName	Identifies the operation, such as Update.
//context.InputParameters["Target"]	Provides the record being updated.
//target.Contains(...)	Checks whether the score was included in the update.
//tracing.Trace()	Records messages about execution.
//InvalidPluginExecutionException	Stops the operation with a friendly error.Code	Purpose
//ITracingService	Writes trace messages.
//IPluginExecutionContext	Reads the current plug-in execution details.
//context.MessageName	Identifies the operation, such as Update.
//context.InputParameters["Target"]	Provides the record being updated.
//target.Contains(...)	Checks whether the score was included in the update.
//tracing.Trace()	Records messages about execution.
//InvalidPluginExecutionException	Stops the operation with a friendly error.