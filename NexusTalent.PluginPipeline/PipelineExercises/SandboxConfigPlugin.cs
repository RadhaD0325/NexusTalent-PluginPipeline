using System;
using Microsoft.Xrm.Sdk;

public class SandboxConfigPlugin : IPlugin
{
    private readonly string _endpoint;
    private readonly string _apiKey;

    public SandboxConfigPlugin(
        string unsecureConfig,
        string secureConfig)
    {
        _endpoint = unsecureConfig;
        _apiKey = secureConfig;

        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            throw new InvalidPluginExecutionException(
                "Endpoint is not configured.");
        }
    }

    public void Execute(IServiceProvider serviceProvider)
    {
        var tracing = (ITracingService)serviceProvider
            .GetService(typeof(ITracingService));

        tracing.Trace(
            "Configuration test started.");

        tracing.Trace(
            "Endpoint configured: {0}",
            !string.IsNullOrWhiteSpace(_endpoint));

        tracing.Trace(
            "API key configured: {0}",
            !string.IsNullOrWhiteSpace(_apiKey));
    }
}