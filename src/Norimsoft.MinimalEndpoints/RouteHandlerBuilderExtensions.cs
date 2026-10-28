using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Norimsoft.MinimalEndpoints.Internal;

namespace Norimsoft.MinimalEndpoints;

/// <summary>
/// Extensions used internally to configure mapped endpoints.
/// </summary>
public static class RouteHandlerBuilderExtensions
{
    internal static RouteHandlerBuilder AddErrorHandler(
        this RouteHandlerBuilder builder,
        ILogger logger,
        bool includeExceptionDetails,
        Func<Exception, HttpContext, IResult>? onError)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            try
            {
                return await next(context);
            }
            catch (InvalidParameterException ex)
            {
                // A client error from RequiredParam, reported like a failed request validation
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParameterName] = [ex.Message],
                });
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception while processing {Method} {Path}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path);

                if (onError is not null)
                {
                    return onError(ex, context.HttpContext);
                }

                // Exception details can leak internals, so only return them in Development
                return includeExceptionDetails
                    ? Results.Problem(
                        type: ex.GetType().FullName,
                        title: ex.Message,
                        detail: ex.StackTrace)
                    : Results.Problem();
            }
        });
    }
}
