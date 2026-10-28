using Microsoft.AspNetCore.Http;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Options for <c>UseMinimalEndpoints</c>.
/// </summary>
public class MinimalEndpointsConfiguration
{
    internal MinimalEndpointsConfiguration()
    {
    }

    /// <summary>
    /// Creates the response for an unhandled exception thrown by an endpoint. The exception is logged before this is called.
    /// </summary>
    /// <remarks>
    /// When not set, a problem details response with status 500 is returned. It includes the exception type, message and
    /// stack trace only when the environment is Development.
    /// </remarks>
    public Func<Exception, HttpContext, IResult>? OnError { get; set; }
}
