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
        
        var freeText = Param("q");
        var price = Param<decimal?>("price");

        if (!string.IsNullOrEmpty(freeText))
        {
            products = products.Where(p => p.Name.Contains(freeText, StringComparison.OrdinalIgnoreCase));
        }
        
        if (price != null)
        {
            products = products.Where(p => p.Price >= price);
        }
        
        
        await Task.CompletedTask;
        return Results.Ok(products);
    }
}
