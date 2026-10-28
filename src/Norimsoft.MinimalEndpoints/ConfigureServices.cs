using System.Reflection;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Selects the assemblies <c>AddMinimalEndpoints</c> scans for endpoints and FluentValidation validators.
/// </summary>
public class ConfigureServices
{
    private readonly List<Type> _types;
    private readonly List<Assembly> _assemblies;
    
    internal ConfigureServices()
    {
        _types = new List<Type>();
        _assemblies = new List<Assembly>();
    }

    /// <summary>Scans the assembly containing <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">Any type from the assembly to scan.</typeparam>
    /// <returns>This instance, to select more assemblies.</returns>
    public ConfigureServices FromAssemblyContaining<T>() =>
        FromAssembly(typeof(T).Assembly);
    
    /// <summary>Scans <paramref name="assembly"/>.</summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>This instance, to select more assemblies.</returns>
    public ConfigureServices FromAssembly(Assembly assembly)
    {
        // TODO: Filter types
        _types.AddRange(assembly.GetTypes());
        _assemblies.Add(assembly);
        return this;
    }

    internal IReadOnlyList<Type> Types => _types;
    internal IReadOnlyList<Assembly> Assemblies => _assemblies;
}
