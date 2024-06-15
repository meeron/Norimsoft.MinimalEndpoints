using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Norimsoft.MinimalEndpoints;

public abstract class MinimalEndpointBase
{
    protected abstract RouteHandlerBuilder Configure(EndpointRoute route);
    protected IResult Ok<TValue>(TValue value) => Results.Ok(value);
    protected IResult Created(string? uri, object? value = null) => Results.Created(uri, value);
    protected IResult NoContent() => Results.NoContent();
    protected IResult BadRequest(object? error) => Results.BadRequest(error);
    
    internal abstract Delegate CreateHandler();
    
    internal RouteHandlerBuilder Configure(WebApplication app)
    {
        return Configure(new EndpointRoute(app, CreateHandler()));
    }
}

public abstract class MinimalEndpoint : MinimalEndpointBase
{
    protected abstract Task<IResult> Handle(CancellationToken ct);

    internal override Delegate CreateHandler()
    {
        var handlerType = GetType();
        return async (IServiceProvider sp, CancellationToken ct) =>
        {
            var endpoint = sp.GetRequiredService(handlerType) as MinimalEndpoint;
            return await endpoint!.Handle(ct);
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
        return async ([AsParameters] TRequest req, IServiceProvider sp, CancellationToken ct) =>
        {
            var endpoint = sp.GetRequiredService(handlerType) as MinimalEndpoint<TRequest>;
            return await endpoint!.Handle(req, ct);
        };
    }
}
