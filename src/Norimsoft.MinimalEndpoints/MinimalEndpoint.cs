using System.Globalization;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Base class for all endpoints. Derive from <see cref="MinimalEndpoint"/> or <see cref="MinimalEndpoint{TRequest}"/> instead of this class.
/// </summary>
public abstract class MinimalEndpointBase
{
    /// <summary>
    /// The <see cref="HttpContext"/> of the request being handled. Only available while handling a request.
    /// </summary>
    protected HttpContext Context { get; private set; } = null!;
    
    /// <summary>
    /// Maps the endpoint's route. Called once at startup by <c>UseMinimalEndpoints</c>.
    /// </summary>
    /// <param name="route">Used to map the route with an HTTP method, e.g. <c>route.Get("/products/{id}")</c>.</param>
    /// <returns>The builder returned by <paramref name="route"/>, which can be further configured, e.g. with <c>.Produces&lt;T&gt;()</c>.</returns>
    protected abstract RouteHandlerBuilder Configure(EndpointRoute route);

    /// <summary>
    /// Gets a route value or, if there is none, the first query string value with the given name.
    /// </summary>
    /// <param name="name">The route parameter or query string key.</param>
    /// <returns>The raw value, or <c>null</c> if it is not present.</returns>
    protected string? Param(string name)
    {
        var value = Context.GetRouteValue(name);
        if (value != null)
        {
            return value.ToString();
        }
        
        return Context.Request.Query.TryGetValue(name, out var values) ? values.First() : null;
    }
    
    /// <summary>
    /// Gets a route value or, if there is none, the first query string value with the given name, converted to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Supported types are <see cref="string"/>, <see cref="int"/>, <see cref="long"/>, <see cref="double"/>, <see cref="decimal"/>,
    /// <see cref="bool"/> and <see cref="Guid"/>, and their nullable variants. <see cref="double"/> and <see cref="decimal"/> are parsed
    /// with the invariant culture.
    /// </remarks>
    /// <param name="name">The route parameter or query string key.</param>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <returns>The converted value, or <c>default</c> if the value is missing, cannot be parsed or <typeparamref name="T"/> is not supported.</returns>
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
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().FullName!);

        return Configure(new EndpointRoute(app, CreateHandler()))
            .AddErrorHandler(logger, app.Environment.IsDevelopment(), config.OnError);
    }

    internal void SetContext(HttpContext ctx) => Context = ctx;
}

/// <summary>
/// Base class for endpoints without a request body. Override <see cref="MinimalEndpointBase.Configure(EndpointRoute)"/>
/// to map the route and <see cref="Handle(CancellationToken)"/> to handle requests.
/// </summary>
/// <remarks>
/// Endpoints are registered as scoped services, so a new instance with its constructor dependencies is created for each request.
/// </remarks>
public abstract class MinimalEndpoint : MinimalEndpointBase
{
    /// <summary>
    /// Handles a request.
    /// </summary>
    /// <param name="ct">Cancelled when the request is aborted.</param>
    /// <returns>The result to send to the client.</returns>
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

/// <summary>
/// Base class for endpoints with a JSON request body bound to <typeparamref name="TRequest"/>. Override
/// <see cref="MinimalEndpointBase.Configure(EndpointRoute)"/> to map the route and <see cref="Handle(TRequest, CancellationToken)"/>
/// to handle requests.
/// </summary>
/// <remarks>
/// If an <c>IValidator&lt;TRequest&gt;</c> from FluentValidation is registered, the request is validated before
/// <see cref="Handle(TRequest, CancellationToken)"/> is called, and a validation problem response is returned when it is invalid.
/// Endpoints are registered as scoped services, so a new instance with its constructor dependencies is created for each request.
/// </remarks>
/// <typeparam name="TRequest">The type of the request body.</typeparam>
public abstract class MinimalEndpoint<TRequest> : MinimalEndpointBase
    where TRequest : class
{
    /// <summary>
    /// Handles a request whose body has been bound and, if a validator is registered, validated.
    /// </summary>
    /// <param name="req">The request body.</param>
    /// <param name="ct">Cancelled when the request is aborted.</param>
    /// <returns>The result to send to the client.</returns>
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
