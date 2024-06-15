using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Endpoints;

public class PatchProduct : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Patch("/products/patch");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        return Task.FromResult(Results.Ok());
    }
}
