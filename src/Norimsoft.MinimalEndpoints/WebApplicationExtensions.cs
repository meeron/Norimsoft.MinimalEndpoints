using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Norimsoft.MinimalEndpoints.Internal;

namespace Norimsoft.MinimalEndpoints;

public static class WebApplicationExtensions
{
    public static void UseMinimalEndpoints(this WebApplication app)
    { 
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        
        foreach (var endpointType in TypesCache.Types)
        {
            var endpoint = (MinimalEndpointBase)sp.GetRequiredService(endpointType);
            endpoint.Configure(app);
        }
        
        // Clear types cache. We don't need it anymore
        TypesCache.Types.Clear();
    }
}
