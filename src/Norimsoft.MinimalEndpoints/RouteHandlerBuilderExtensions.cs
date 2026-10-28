using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Norimsoft.MinimalEndpoints;

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
