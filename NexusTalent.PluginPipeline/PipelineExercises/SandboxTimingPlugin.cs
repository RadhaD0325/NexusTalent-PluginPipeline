using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xrm.Sdk;

public class SandboxTimingPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        var sw = Stopwatch.StartNew();
        try
        {
            tracing.Trace("Timing test started");

            //simulate a slow operation for 3 seconds

            Thread.Sleep(3000);
            sw.Stop();

            tracing.Trace("Execution completed in {0} ms.",
            sw.ElapsedMilliseconds);

        }
        catch(Exception ex)
        {
            sw.Stop();
            tracing.Trace("Timing test failed :{0}", ex.ToString());

            throw new InvalidPluginExecutionException("Sandbox Timing test failed.", ex);
        }   
    }
}