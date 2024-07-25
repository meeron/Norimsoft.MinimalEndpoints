using Benchmark.Common;
using Norimsoft.MinimalEndpoints;

namespace Benchmark.App.MinimalEndpoints.Endpoints;

public class GetProductsEndpoint : MinimalEndpoint
{
    private readonly ProductsRepository _products;

    public GetProductsEndpoint(ProductsRepository products)
    {
        _products = products;
    }

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/products")
            .Produces<Product[]>()
            .WithOpenApi();
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        var count = Param("count", 10);
        var products = _products.GetAll(count);

        return Task.FromResult(Results.Ok(products));
    }
}
