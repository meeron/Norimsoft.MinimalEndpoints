using Microsoft.AspNetCore.Mvc;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class UpdateProduct : MinimalEndpoint<UpdateProductReq>
{
    private readonly ProductsRepository _products;

    public UpdateProduct(ProductsRepository products)
    {
        _products = products;
    }

    protected override void Configure(EndpointRoute route)
    {
        route.Put("/products/{id}");
    }

    protected override async Task<IResult> Handle(UpdateProductReq req, CancellationToken ct)
    {
        await Task.CompletedTask;

        var updatedCount = _products.Update(req.Id, req.Body.Name, req.Body.Price);

        return updatedCount > 0
            ? Ok(_products.Get(req.Id))
            : BadRequest(new { message = "Failed to update" });
    }
}

public class UpdateProductReq
{
    [FromRoute]
    public Guid Id { get; init; }
    [FromBody]
    public required UpdateProductBody Body { get; init; }
}

public record UpdateProductBody(string Name, decimal Price);
