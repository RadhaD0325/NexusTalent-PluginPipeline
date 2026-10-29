using System;
using System.IO;
using Microsoft.Xrm.Sdk;

public class SandboxFileTestPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var tracing = (ITracingService)serviceProvider
            .GetService(typeof(ITracingService));

        try
        {
            tracing.Trace("Attempting file-write test.");

            File.WriteAllText(
                @"C:\Temp\SandboxTest.txt",
                "Sandbox test");

            tracing.Trace(
                "Sandbox test executed at {0}",
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            tracing.Trace(
                "File write failed: {0}",
                ex.ToString());

            throw new InvalidPluginExecutionException(
                "File-write test failed in the sandbox.", ex);
        }
    }
}