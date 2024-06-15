using System.Reflection;

namespace Norimsoft.MinimalEndpoints;

public class ConfigureServices
{
    private readonly List<Type> _types;
    
    internal ConfigureServices()
    {
        _types = new List<Type>();
    }

    public ConfigureServices FromAssemblyContaining<T>() =>
        FromAssembly(typeof(T).Assembly);
    
    public ConfigureServices FromAssembly(Assembly assembly)
    {
        // TODO: Filter types
        _types.AddRange(assembly.GetTypes());
        return this;
    }

    internal IReadOnlyList<Type> Types => _types;
}
