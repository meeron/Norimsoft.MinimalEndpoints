using System.Reflection;

namespace Norimsoft.MinimalEndpoints;

public class ConfigureServices
{
    private readonly List<Type> _types;
    private readonly List<Assembly> _assemblies;
    
    internal ConfigureServices()
    {
        _types = new List<Type>();
        _assemblies = new List<Assembly>();
    }

    public ConfigureServices FromAssemblyContaining<T>() =>
        FromAssembly(typeof(T).Assembly);
    
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
