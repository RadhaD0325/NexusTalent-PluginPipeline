using System;
using Microsoft.Xrm.Sdk;

namespace NexusTalent.PluginPipeline.PipelineExercises
{
    public static class PluginTraceHelper
    {
        public static void TraceContext(
            ITracingService tracing,
            IPluginExecutionContext context)
        {
            tracing.Trace("========== PLUGIN EXECUTION ==========");
            tracing.Trace("MessageName: {0}", context.MessageName);
            tracing.Trace("PrimaryEntityName: {0}", context.PrimaryEntityName);
            tracing.Trace("PrimaryEntityId: {0}", context.PrimaryEntityId);
            tracing.Trace("Stage: {0}", context.Stage);
            tracing.Trace("Mode: {0}", context.Mode);
            tracing.Trace("IsInTransaction: {0}", context.IsInTransaction);
            tracing.Trace("Depth: {0}", context.Depth);
            tracing.Trace("CorrelationId: {0}", context.CorrelationId);
            tracing.Trace("======================================");
        }
    }
}