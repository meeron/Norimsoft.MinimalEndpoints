using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Norimsoft.MinimalEndpoints.Internal;

namespace Norimsoft.MinimalEndpoints;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMinimalEndpoints(this IServiceCollection services)
    {
        var asm = Assembly.GetCallingAssembly();
        return services.AddMinimalEndpoints(c => c.FromAssembly(asm));
    }
    
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
            TypesCache.Types.Add(type);
        }

        // Add validators
        AssemblyScanner.FindValidatorsInAssemblies(config.Assemblies)
            .ForEach(scanResult =>
            {
                var descriptor = new ServiceDescriptor(
                    serviceType: scanResult.InterfaceType,
                    implementationType: scanResult.ValidatorType,
                    lifetime: ServiceLifetime.Transient);
                services.TryAdd(descriptor);
            });
        
        return services;
    }
}
