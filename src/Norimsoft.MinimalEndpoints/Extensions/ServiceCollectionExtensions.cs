using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace Norimsoft.MinimalEndpoints;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMinimalEndpoints(this IServiceCollection services) =>
        services.AddMinimalEndpoints(c => c.FromAssembly(Assembly.GetEntryAssembly()!));
    
    public static IServiceCollection AddMinimalEndpoints(
        this IServiceCollection services,
        Action<ConfigureServices> configure)
    {
        var config = new ConfigureServices();
        configure(config);
        
        // Find all endpoint types in the assembly
        var endpointsTypes = config.Types
            .Where(t => t.BaseType == typeof(MinimalEndpoint))
            .ToList();
        var endpointsWithRequests = config.Types
            .Where(t => t.BaseType is { IsGenericType: true } && 
                        t.BaseType.GetGenericTypeDefinition() == typeof(MinimalEndpoint<>))
            .ToList();
        
        endpointsTypes.AddRange(endpointsWithRequests);

        foreach (var type in endpointsTypes)
        {
            // Used in requests handlers
            services.AddScoped(type);
            
            // Used by UseMinimalEndpoints extension
            services.AddTransient(typeof(MinimalEndpointBase), type);
        }
        
        return services;
    }
}
