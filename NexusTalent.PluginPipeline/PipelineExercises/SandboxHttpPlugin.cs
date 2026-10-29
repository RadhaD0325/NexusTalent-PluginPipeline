using System;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Xrm.Sdk;

public class SandboxHttpPlugin : IPlugin
{
    private static readonly HttpClient Http =
        new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

    public void Execute(IServiceProvider serviceProvider)
    {
        var tracing = (ITracingService)serviceProvider
            .GetService(typeof(ITracingService));

        var sw = Stopwatch.StartNew();

        try
        {
            tracing.Trace("HTTP request started.");

            using (var response = Http.GetAsync(
                "https://www.microsoft.com")
                .GetAwaiter().GetResult())
            {
                tracing.Trace(
                    "HTTP status: {0}",
                    (int)response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            tracing.Trace(
                "HTTP request failed: {0}",
                ex.ToString());

            throw new InvalidPluginExecutionException(
                "External HTTP request failed.", ex);
        }
        finally
        {
            sw.Stop();
            tracing.Trace(
                "Elapsed time: {0} ms",
                sw.ElapsedMilliseconds);
        }
    }
}