using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Norimsoft.MinimalEndpoints.Internal;

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
    /// A missing or invalid value gives <c>default</c>; use <see cref="TryParam{T}"/> or <see cref="RequiredParam{T}"/> to detect them.
    /// </summary>
    /// <remarks>
    /// Supported types are <see cref="string"/>, enums and any type implementing <see cref="IParsable{TSelf}"/>, e.g. all numeric
    /// types, <see cref="bool"/>, <see cref="Guid"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, <see cref="DateOnly"/>,
    /// <see cref="TimeOnly"/> and <see cref="TimeSpan"/>, and their nullable variants. Values are parsed with the invariant culture.
    /// Enums are matched by name, ignoring case, or by a defined numeric value.
    /// </remarks>
    /// <param name="name">The route parameter or query string key.</param>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <returns>The converted value, or <c>default</c> if the value is missing or cannot be parsed.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not a supported type.</exception>
    protected T? Param<T>(string name)
    {
        var raw = Param(name);

        // Unlike the other types, an empty string is a valid string value here
        if (typeof(T) == typeof(string))
        {
            return (T?)(object?)raw;
        }

        return ParamParser.Parse<T>(raw, out var value) == ParamParseStatus.Success ? value : default;
    }

    /// <summary>
    /// Gets a route value or, if there is none, the first query string value with the given name, converted to <typeparamref name="T"/>.
    /// See <see cref="Param{T}"/> for the supported types.
    /// </summary>
    /// <param name="name">The route parameter or query string key.</param>
    /// <param name="value">The converted value, or <c>default</c> when this method returns <c>false</c>.</param>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <returns><c>true</c> if the value is present and valid; <c>false</c> if it is missing, empty or cannot be parsed.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not a supported type.</exception>
    protected bool TryParam<T>(string name, [MaybeNullWhen(false)] out T value)
    {
        if (ParamParser.Parse<T>(Param(name), out var parsed) == ParamParseStatus.Success)
        {
            value = parsed!;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Gets a route value or, if there is none, the first query string value with the given name, converted to <typeparamref name="T"/>.
    /// If the value is missing, empty or cannot be parsed, the request ends with a 400 validation problem response naming the parameter.
    /// See <see cref="Param{T}"/> for the supported types.
    /// </summary>
    /// <remarks>
    /// The request is ended by throwing an exception that the endpoint's error handler turns into the 400 response, so don't catch
    /// all exceptions around this call.
    /// </remarks>
    /// <param name="name">The route parameter or query string key.</param>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not a supported type.</exception>
    protected T RequiredParam<T>(string name)
    {
        return ParamParser.Parse<T>(Param(name), out var value) switch
        {
            ParamParseStatus.Success => value!,
            ParamParseStatus.Missing => throw new InvalidParameterException(name, $"The '{name}' parameter is required."),
            _ => throw new InvalidParameterException(name, $"The value of the '{name}' parameter is not valid."),
        };
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
