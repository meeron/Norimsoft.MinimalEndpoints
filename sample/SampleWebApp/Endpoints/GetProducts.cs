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

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/products")
            .Produces<Product[]>();
    }

    protected override async Task<IResult> Handle(CancellationToken ct)
    {
        var products = _products.GetAll();
        
        await Task.CompletedTask;
        return Results.Ok(products);
    }
}
