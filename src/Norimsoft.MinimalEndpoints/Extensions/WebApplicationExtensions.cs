using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Norimsoft.MinimalEndpoints;

public static class WebApplicationExtensions
{
    public static void UseMinimalEndpoints(this WebApplication app)
    { 
        using var scope = app.Services.CreateScope();
        var endpoints = scope.ServiceProvider.GetServices(typeof(MinimalEndpointBase));
        
        foreach (var end in endpoints)
        {
            (end as MinimalEndpointBase)!.Configure(app);
        }
    }
}
