using Microsoft.AspNetCore.Mvc;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Models;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class AddProduct : MinimalEndpoint<AddProductReq>
{
    private readonly ProductsRepository _products;

    public AddProduct(ProductsRepository products)
    {
        _products = products;
    }

    protected override void Configure(EndpointRoute route)
    {
        route.Post("/products");
    }

    protected override async Task<IResult> Handle(AddProductReq req, CancellationToken ct)
    {
        await Task.CompletedTask;

        var id = Guid.NewGuid();
        _products.Add(new Product
        {
            Id = id,
            Name = req.Body.Name,
            Price = req.Body.Price,
        });
        
        return Created($"/products/{id}");
    }
}

public class AddProductReq
{
    [FromBody]
    public required NewProduct Body { get; init; }
}

public record NewProduct(string Name, decimal Price);
