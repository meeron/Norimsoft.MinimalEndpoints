using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Norimsoft.MinimalEndpoints;

public static class WebApplicationExtensions
{
    public static void UseMinimalEndpoints(this WebApplication app)
    {
        var assembly = Assembly.GetEntryAssembly();
        
        var endpointsTypes = assembly!.GetTypes()
            .Where(t => t.BaseType == typeof(MinimalEndpoint))
            .ToList();
        var endpointsWithRequests = assembly.GetTypes()
            .Where(t => t.BaseType is { IsGenericType: true } && 
                        t.BaseType.GetGenericTypeDefinition() == typeof(MinimalEndpoint<>))
            .ToList();
        
        endpointsTypes.AddRange(endpointsWithRequests);

        using var scope = app.Services.CreateScope();
        
        foreach (var type in endpointsTypes)
        {
            var end = scope.ServiceProvider.GetService(type) as MinimalEndpointBase;
            end!.Configure(app);
        }
    }
    
}
