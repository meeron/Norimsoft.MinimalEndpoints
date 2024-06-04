using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Norimsoft.MinimalEndpoints;

public abstract class MinimalEndpointBase
{
    protected abstract void Configure(EndpointRoute route);
    protected IResult Ok<TValue>(TValue value) => Results.Ok(value);
    protected IResult Created(string? uri) => Results.Created(uri, null);
    protected IResult NoContent() => Results.NoContent();
    
    internal abstract Delegate CreateHandler();
    
    internal void Configure(WebApplication app)
    {
        Configure(new EndpointRoute(app, CreateHandler()));
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
