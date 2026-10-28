using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Maps an endpoint's route. Passed to <see cref="MinimalEndpointBase.Configure(EndpointRoute)"/>.
/// </summary>
public class EndpointRoute
{
    private readonly WebApplication _app;
    private readonly Delegate _handler;

    internal EndpointRoute(WebApplication app, Delegate handler)
    {
        _app = app;
        _handler = handler;
    }

    /// <summary>Maps the endpoint to GET requests matching <paramref name="pattern"/>.</summary>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for further configuring the endpoint.</returns>
    public RouteHandlerBuilder Get([StringSyntax("Route")] string pattern) =>
        _app.MapGet(pattern, _handler);
    
    /// <summary>Maps the endpoint to POST requests matching <paramref name="pattern"/>.</summary>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for further configuring the endpoint.</returns>
    public RouteHandlerBuilder Post([StringSyntax("Route")] string pattern) =>
        _app.MapPost(pattern, _handler);
    
    /// <summary>Maps the endpoint to PUT requests matching <paramref name="pattern"/>.</summary>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for further configuring the endpoint.</returns>
    public RouteHandlerBuilder Put([StringSyntax("Route")] string pattern) =>
        _app.MapPut(pattern, _handler);
    
    /// <summary>Maps the endpoint to DELETE requests matching <paramref name="pattern"/>.</summary>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for further configuring the endpoint.</returns>
    public RouteHandlerBuilder Delete([StringSyntax("Route")] string pattern) =>
        _app.MapDelete(pattern, _handler);
    
    /// <summary>Maps the endpoint to PATCH requests matching <paramref name="pattern"/>.</summary>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>A builder for further configuring the endpoint.</returns>
    public RouteHandlerBuilder Patch([StringSyntax("Route")] string pattern) =>
        _app.MapPatch(pattern, _handler);
    
    /// <summary>Maps the endpoint to requests with the given HTTP <paramref name="method"/> matching <paramref name="pattern"/>, e.g. HEAD.</summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="method">The HTTP method.</param>
    /// <returns>A builder for further configuring the endpoint.</returns>
    public RouteHandlerBuilder Method([StringSyntax("Route")] string pattern, HttpMethod method) =>
        _app.MapMethods(pattern, [method.Method], _handler);
}
