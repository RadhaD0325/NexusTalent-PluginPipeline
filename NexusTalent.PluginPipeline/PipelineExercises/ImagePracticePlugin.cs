
using System;
using Microsoft.Xrm.Sdk;

public class ImagePracticePlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var context = (IPluginExecutionContext)
            serviceProvider.GetService(
                typeof(IPluginExecutionContext));

        var tracing = (ITracingService)
            serviceProvider.GetService(
                typeof(ITracingService));

        tracing.Trace(
            "Message: {0}, Stage: {1}, Mode: {2}",
            context.MessageName,
            context.Stage,
            context.Mode);

        // Inspect Target
        if (context.InputParameters.Contains("Target"))
        {
            object targetObject = context.InputParameters["Target"];

            if (targetObject is Entity target)
            {
                tracing.Trace("Target ID: {0}", target.Id);

                foreach (var attribute in target.Attributes)
                {
                    tracing.Trace(
                        "Target: {0} = {1}",
                        attribute.Key,
                        attribute.Value ?? "<null>");
                }
            }
            else if (targetObject is EntityReference reference)
            {
                tracing.Trace(
                    "Target is EntityReference. ID: {0}",
                    reference.Id);
            }
        }

        // Inspect Pre-Image
        if (context.PreEntityImages.Contains("PreImage"))
        {
            Entity pre = context.PreEntityImages["PreImage"];

            foreach (var attribute in pre.Attributes)
            {
                tracing.Trace(
                    "PreImage: {0} = {1}",
                    attribute.Key,
                    attribute.Value ?? "<null>");
            }
        }
        else
        {
            tracing.Trace("PreImage is not available.");
        }

        // Inspect Post-Image
        if (context.PostEntityImages.Contains("PostImage"))
        {
            Entity post = context.PostEntityImages["PostImage"];

            foreach (var attribute in post.Attributes)
            {
                tracing.Trace(
                    "PostImage: {0} = {1}",
                    attribute.Key,
                    attribute.Value ?? "<null>");
            }
        }
        else
        {
            tracing.Trace("PostImage is not available.");
        }
    }
}
