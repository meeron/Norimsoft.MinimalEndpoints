using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Endpoints;

public class GetWithError : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/error");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        throw new Exception("Test error");
    }
}
