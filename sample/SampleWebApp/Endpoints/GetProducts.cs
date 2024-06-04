using Norimsoft.MinimalEndpoints;
using SampleWebApp.Models;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class GetProducts : MinimalEndpoint
{
    private readonly ProductsRepository _products;

    public GetProducts(ProductsRepository products)
    {
        _products = products;
    }

    protected override void Configure(EndpointRoute route)
    {
        route.Get("/products")
            .Produces<Product[]>();
    }

    protected override async Task<IResult> Handle(CancellationToken ct)
    {
        await Task.CompletedTask;

        var products = _products.GetAll();

        return Ok(products);
    }
}
