namespace Norimsoft.MinimalEndpoints.Internal;

/// <summary>
/// Thrown by RequiredParam when a parameter is missing or invalid. The endpoint's error handler turns it into a
/// 400 validation problem response.
/// </summary>
internal sealed class InvalidParameterException(string parameterName, string message) : Exception(message)
{
    public string ParameterName { get; } = parameterName;
}
