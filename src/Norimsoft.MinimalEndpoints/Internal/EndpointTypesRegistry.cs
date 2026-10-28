namespace Norimsoft.MinimalEndpoints.Internal;

/// <summary>
/// Endpoint types found by AddMinimalEndpoints, registered as a singleton so each app has its own list.
/// </summary>
internal sealed class EndpointTypesRegistry
{
    private readonly List<Type> _types = [];

    public IReadOnlyList<Type> Types => _types;

    public bool Add(Type type)
    {
        if (_types.Contains(type))
        {
            return false;
        }

        _types.Add(type);
        return true;
    }

    public void Clear() => _types.Clear();
}
