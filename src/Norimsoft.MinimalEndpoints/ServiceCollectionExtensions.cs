using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Norimsoft.MinimalEndpoints.Internal;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Registers Minimal Endpoints services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all endpoints and FluentValidation validators from the calling assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMinimalEndpoints(this IServiceCollection services)
    {
        var asm = Assembly.GetCallingAssembly();
        return services.AddMinimalEndpoints(c => c.FromAssembly(asm));
    }
    
    /// <summary>
    /// Registers all endpoints and FluentValidation validators from the selected assemblies.
    /// Can be called several times; each endpoint is registered only once.
    /// </summary>
    /// <example>
    /// <code>services.AddMinimalEndpoints(c => c.FromAssemblyContaining&lt;Book&gt;());</code>
    /// </example>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Selects the assemblies to scan.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMinimalEndpoints(
        this IServiceCollection services,
        Action<ConfigureServices> configure)
    {
        var config = new ConfigureServices();
        configure(config);
        
        var registry = GetOrAddRegistry(services);

        // Find all concrete endpoint types in the assembly, including ones derived from abstract base endpoints
        var endpointsTypes = config.Types
            .Where(t => t is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false } &&
                        t.IsAssignableTo(typeof(MinimalEndpointBase)));

        foreach (var type in endpointsTypes)
        {
            // Skip types already registered by an earlier call, so each endpoint is mapped once
            if (registry.Add(type))
            {
                // Used in requests handlers
                services.AddScoped(type);
            }
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

    private static EndpointTypesRegistry GetOrAddRegistry(IServiceCollection services)
    {
        var registry = services
            .Where(d => !d.IsKeyedService && d.ServiceType == typeof(EndpointTypesRegistry))
            .Select(d => d.ImplementationInstance)
            .OfType<EndpointTypesRegistry>()
            .FirstOrDefault();

        if (registry is null)
        {
            registry = new EndpointTypesRegistry();
            services.AddSingleton(registry);
        }

        return registry;
    }
}
