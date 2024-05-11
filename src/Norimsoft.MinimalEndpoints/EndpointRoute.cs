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
}
