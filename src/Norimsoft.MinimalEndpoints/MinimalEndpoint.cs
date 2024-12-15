using System.Globalization;
using FluentValidation;
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

    protected string? Param(string name)
    {
        var value = Context.GetRouteValue(name);
        if (value != null)
        {
            return value.ToString();
        }
        
        return Context.Request.Query.TryGetValue(name, out var values) ? values.First() : null;
    }
    
    protected T? Param<T>(string name)
    {
        var value = Param(name);
        if (value == null)
        {
            return default;
        }
        
        var outType = typeof(T);

        if (outType == typeof(string))
        {
            return (T)Convert.ChangeType(value, typeof(string));
        }

        if ((outType == typeof(int) || outType == typeof(int?))
            && int.TryParse(value, out var intValue))
        {
            return (T)Convert.ChangeType(intValue, typeof(int));
        }
        
        if ((outType == typeof(Guid) || outType == typeof(Guid?))
            && Guid.TryParse(value, out var guidValue))
        {
            return (T)Convert.ChangeType(guidValue, typeof(Guid));
        }
        
        if ((outType == typeof(bool) || outType == typeof(bool?))
            && bool.TryParse(value, out var boolValue))
        {
            return (T)Convert.ChangeType(boolValue, typeof(bool));
        }
        
        if ((outType == typeof(long) || outType == typeof(long?))
            && long.TryParse(value, out var longValue))
        {
            return (T)Convert.ChangeType(longValue, typeof(long));
        }
        
        if ((outType == typeof(double) || outType == typeof(double?))
            && double.TryParse(value, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return (T)Convert.ChangeType(doubleValue, typeof(double));
        }
        
        if ((outType == typeof(decimal) || outType == typeof(decimal?))
            && decimal.TryParse(value, CultureInfo.InvariantCulture, out var decimalValue))
        {
            return (T)Convert.ChangeType(decimalValue, typeof(decimal));
        }

        return default;
    }
    
    internal abstract Delegate CreateHandler();
    
    internal RouteHandlerBuilder Configure(WebApplication app, MinimalEndpointsConfiguration config)
    {
        return Configure(new EndpointRoute(app, CreateHandler()))
            .AddErrorHandler(GetType().FullName!, config.OnError);
    }

    internal void SetContext(HttpContext ctx) => Context = ctx;
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
        return async ([FromBody] TRequest req, IServiceProvider sp, CancellationToken ct, HttpContext ctx) =>
        {
            var endpoint = (MinimalEndpoint<TRequest>)sp.GetRequiredService(handlerType);
            
            // TODO: Support for pipelines/middlewares
            var validator = sp.GetService<IValidator<TRequest>>();
            if (validator != null)
            {
                var result = await validator.ValidateAsync(req, ct);
                if (!result.IsValid)
                {
                    return Results.ValidationProblem(result.ToDictionary());
                }
            }
            
            endpoint.SetContext(ctx);

            return await endpoint.Handle(req, ct);
        };
    }
}
