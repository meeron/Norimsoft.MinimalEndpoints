using System.Reflection;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Selects the assemblies <c>AddMinimalEndpoints</c> scans for endpoints and FluentValidation validators.
/// </summary>
public class ConfigureServices
{
    private readonly List<Type> _types;

    internal ConfigureServices()
    {
        _types = new List<Type>();
    }

    /// <summary>Scans the assembly containing <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">Any type from the assembly to scan.</typeparam>
    /// <returns>This instance, to select more assemblies.</returns>
    public ConfigureServices FromAssemblyContaining<T>() =>
        FromAssembly(typeof(T).Assembly);
    
    /// <summary>Scans <paramref name="assembly"/>.</summary>
    /// <remarks>
    /// Types that cannot be loaded, e.g. because they depend on a missing assembly, are skipped.
    /// </remarks>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>This instance, to select more assemblies.</returns>
    public ConfigureServices FromAssembly(Assembly assembly)
    {
        _types.AddRange(GetLoadableTypes(assembly));
        return this;
    }

    internal IReadOnlyList<Type> Types => _types;

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Types that failed to load are null, keep the rest
            return ex.Types.OfType<Type>();
        }
    }
}
