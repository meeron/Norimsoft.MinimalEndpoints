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
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var configuration = new MinimalEndpointsConfiguration();
        config(configuration);
        
        foreach (var endpointType in TypesCache.Types)
        {
            var endpoint = (MinimalEndpointBase)sp.GetRequiredService(endpointType);
            endpoint.Configure(app, configuration);
        }
        
        // Clear types cache. We don't need it anymore
        TypesCache.Types.Clear();
    }
}
