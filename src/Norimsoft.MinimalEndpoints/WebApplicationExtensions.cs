using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Norimsoft.MinimalEndpoints.Internal;

namespace Norimsoft.MinimalEndpoints;

public static class WebApplicationExtensions
{
    public static void UseMinimalEndpoints(this WebApplication app) =>
        app.UseMinimalEndpoints(_ => { });
    
    public static void UseMinimalEndpoints(this WebApplication app, Action<MinimalEndpointsConfiguration> config)
    {
        var registry = app.Services.GetService<EndpointTypesRegistry>();
        if (registry is null)
        {
            // AddMinimalEndpoints was not called, nothing to map
            return;
        }

        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var configuration = new MinimalEndpointsConfiguration();
        config(configuration);

        foreach (var endpointType in registry.Types)
        {
            var endpoint = (MinimalEndpointBase)sp.GetRequiredService(endpointType);
            endpoint.Configure(app, configuration);
        }

        // Clear registered types. We don't need them anymore
        registry.Clear();
    }
}
