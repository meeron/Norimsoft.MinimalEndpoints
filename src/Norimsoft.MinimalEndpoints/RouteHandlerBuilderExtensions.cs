using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Norimsoft.MinimalEndpoints;

public static class RouteHandlerBuilderExtensions
{
    internal static RouteHandlerBuilder AddErrorHandler(this RouteHandlerBuilder builder, string endpointType)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var loggerFactory = (ILoggerFactory)context.HttpContext.RequestServices.GetRequiredService(typeof(ILoggerFactory));
            var logger = loggerFactory.CreateLogger(endpointType);
            
            try
            {
                return await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Results.Problem(
                    type: ex.GetType().FullName,
                    title: ex.Message,
                    detail: ex.StackTrace);
            }
        });
    }
}
