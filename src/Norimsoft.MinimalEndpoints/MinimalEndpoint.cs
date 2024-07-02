using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Norimsoft.MinimalEndpoints;

public abstract class MinimalEndpointBase
{
    protected HttpContext Context { get; private set; }
    
    protected abstract RouteHandlerBuilder Configure(EndpointRoute route);

    protected T? Param<T>(string name, T? fallback = default)
        where T: IComparable
    {
        var value = Context.GetRouteValue(name);
        if (value == null)
        {
            return Query(name, fallback);
        }

        return TryBindValue<T>(value.ToString(), out var outValue)
            ? outValue
            : fallback;
    }
    
    internal abstract Delegate CreateHandler();
    
    internal RouteHandlerBuilder Configure(WebApplication app, MinimalEndpointsConfiguration config)
    {
        return Configure(new EndpointRoute(app, CreateHandler()))
            .AddErrorHandler(GetType().FullName!, config.OnError);
    }

    internal void SetContext(HttpContext ctx) => Context = ctx;

    private T? Query<T>(string name, T? fallback = default)
        where T: IComparable
    {
        if (Context.Request.Query.TryGetValue(name, out var values))
        {
            // TODO: Bind collection
            return TryBindValue<T>(values.First(), out var val)
                ? val
                : fallback;
        }
        
        return fallback;
    }

    private static bool TryBindValue<T>(string? value, out T? outValue)
        where T: IComparable
    {
        var outType = typeof(T);

        if (outType == typeof(string))
        {
            outValue = (T)Convert.ChangeType(value!, typeof(string));
            return true;
        }

        if (outType == typeof(int) && int.TryParse(value, out var intValue))
        {
            outValue = (T)Convert.ChangeType(intValue, typeof(int));
            return true;
        }
        
        if (outType == typeof(Guid) && Guid.TryParse(value, out var guidValue))
        {
            outValue = (T)Convert.ChangeType(guidValue, typeof(Guid));
            return true;
        }
        
        if (outType == typeof(bool) && bool.TryParse(value, out var boolValue))
        {
            outValue = (T)Convert.ChangeType(boolValue, typeof(bool));
            return true;
        }
        
        if (outType == typeof(long) && long.TryParse(value, out var longValue))
        {
            outValue = (T)Convert.ChangeType(longValue, typeof(long));
            return true;
        }
        
        if (outType == typeof(double) && double.TryParse(value, CultureInfo.InvariantCulture, out var doubleValue))
        {
            outValue = (T)Convert.ChangeType(doubleValue, typeof(double));
            return true;
        }
        
        outValue = default;
        return false;
    }
}

public abstract class MinimalEndpoint : MinimalEndpointBase
{
    protected abstract Task<IResult> Handle(CancellationToken ct);

    internal override Delegate CreateHandler()
    {
        var handlerType = GetType();
        return (IServiceProvider sp, CancellationToken ct, HttpContext ctx) =>
        {
            var endpoint = (MinimalEndpoint)sp.GetRequiredService(handlerType);
            endpoint.SetContext(ctx);
            
            return endpoint.Handle(ct);
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
        return ([FromBody] TRequest req, IServiceProvider sp, CancellationToken ct, HttpContext ctx) =>
        {
            var endpoint = (MinimalEndpoint<TRequest>)sp.GetRequiredService(handlerType);
            endpoint.SetContext(ctx);

            return endpoint.Handle(req, ct);
        };
    }
}
