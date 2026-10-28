using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Norimsoft.MinimalEndpoints.Internal;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Maps Minimal Endpoints.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps all endpoints registered with <c>AddMinimalEndpoints</c>, using the default configuration.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void UseMinimalEndpoints(this WebApplication app) =>
        app.UseMinimalEndpoints(_ => { });
    
    /// <summary>
    /// Maps all endpoints registered with <c>AddMinimalEndpoints</c>.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="config">Configures options such as <see cref="MinimalEndpointsConfiguration.OnError"/>.</param>
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
