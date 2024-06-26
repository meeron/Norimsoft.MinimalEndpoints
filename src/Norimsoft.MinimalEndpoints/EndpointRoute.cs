using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;

namespace Norimsoft.MinimalEndpoints;

public class EndpointRoute
{
    private readonly WebApplication _app;
    private readonly Delegate _handler;

    internal EndpointRoute(WebApplication app, Delegate handler)
    {
        _app = app;
        _handler = handler;
    }

    public RouteHandlerBuilder Get([StringSyntax("Route")] string pattern) =>
        _app.MapGet(pattern, _handler);
    
    public RouteHandlerBuilder Post([StringSyntax("Route")] string pattern) =>
        _app.MapPost(pattern, _handler);
    
    public RouteHandlerBuilder Put([StringSyntax("Route")] string pattern) =>
        _app.MapPut(pattern, _handler);
    
    public RouteHandlerBuilder Delete([StringSyntax("Route")] string pattern) =>
        _app.MapDelete(pattern, _handler);
    
    public RouteHandlerBuilder Patch([StringSyntax("Route")] string pattern) =>
        _app.MapPatch(pattern, _handler);
    
    public RouteHandlerBuilder Method([StringSyntax("Route")] string pattern, HttpMethod method) =>
        _app.MapMethods(pattern, [method.Method], _handler);
}
