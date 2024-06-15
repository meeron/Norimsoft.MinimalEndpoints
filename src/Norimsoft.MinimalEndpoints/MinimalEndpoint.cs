using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Norimsoft.MinimalEndpoints;

public abstract class MinimalEndpointBase
{
    protected HttpContext Context { get; private set; }
    
    protected abstract RouteHandlerBuilder Configure(EndpointRoute route);
    
    internal abstract Delegate CreateHandler();
    
    internal RouteHandlerBuilder Configure(WebApplication app)
    {
        return Configure(new EndpointRoute(app, CreateHandler()));
    }

    internal void SetContext(HttpContext ctx) => Context = ctx;
}

public abstract class MinimalEndpoint : MinimalEndpointBase
{
    protected abstract Task<IResult> Handle(CancellationToken ct);

    internal override Delegate CreateHandler()
    {
        var handlerType = GetType();
        return async (IServiceProvider sp, CancellationToken ct, HttpContext ctx) =>
        {
            var endpoint = (MinimalEndpoint)sp.GetRequiredService(handlerType);
            endpoint.SetContext(ctx);
            
            return await endpoint.Handle(ct);
        };
    }
}

public abstract class MinimalEndpoint<TRequest> : MinimalEndpointBase
    where TRequest : class
{
    protected abstract Task<IResult> Handle(TRequest req, CancellationToken ct);

    internal override Delegate CreateHandler()
    {
        var handlerType = GetType();
        return async ([AsParameters] TRequest req, IServiceProvider sp, CancellationToken ct, HttpContext ctx) =>
        {
            var endpoint = (MinimalEndpoint<TRequest>)sp.GetRequiredService(handlerType);
            endpoint.SetContext(ctx);
            
            return await endpoint.Handle(req, ct);
        };
    }
}
