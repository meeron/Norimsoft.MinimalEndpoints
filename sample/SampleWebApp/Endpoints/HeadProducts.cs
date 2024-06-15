using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Endpoints;

public class HeadProducts : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Method("/products/head", HttpMethod.Head);
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        Context.Response.Headers.Append("X-Test", "Test");
        return Task.FromResult(Results.NoContent());
    }
}
