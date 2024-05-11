using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Norimsoft.MinimalEndpoints;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMinimalEndpointsFromAssemblyContaining<T>(
        this IServiceCollection services)
    {
        // Get the assembly containing the specified type
        var assembly = typeof(T).GetTypeInfo().Assembly;
        
        // Find all endpoint types in the assembly
        var endpointsTypes = assembly!.GetTypes()
            .Where(t => t.BaseType == typeof(MinimalEndpoint))
            .ToList();
        var endpointsWithRequests = assembly.GetTypes()
            .Where(t => t.BaseType is { IsGenericType: true } && 
                        t.BaseType.GetGenericTypeDefinition() == typeof(MinimalEndpoint<>))
            .ToList();
        
        endpointsTypes.AddRange(endpointsWithRequests);

        foreach (var type in endpointsTypes)
        {
            services.AddScoped(type);
        }
        
        return services;
    }
}
