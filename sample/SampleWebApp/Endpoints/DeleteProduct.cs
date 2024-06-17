using Norimsoft.MinimalEndpoints;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class DeleteProduct : MinimalEndpoint
{
    private readonly ProductsRepository _products;

    public DeleteProduct(ProductsRepository products)
    {
        _products = products;
    }

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Delete("/products/{id}");
    }

    protected override async Task<IResult> Handle(CancellationToken ct)
    {
        var deletedCount = _products.Delete(Param<Guid>("id"));
        
        await Task.CompletedTask;
        return Results.Ok(new { deletedCount });
    }
}
