using Norimsoft.MinimalEndpoints;
using SampleWebApp.Models;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class AddProduct : MinimalEndpoint<NewProduct>
{
    private readonly ProductsRepository _products;

    public AddProduct(ProductsRepository products)
    {
        _products = products;
    }

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Post("/products");
    }

    protected override async Task<IResult> Handle(NewProduct req, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        var newProduct = new Product
        {
            Id = id,
            Name = req.Name,
            Price = req.Price,
        };
        _products.Add(newProduct);
        
        await Task.CompletedTask;
        return Results.Created($"/products/{id}", newProduct);
    }
}

public record NewProduct(string Name, decimal Price);
