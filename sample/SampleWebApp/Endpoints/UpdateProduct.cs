using Microsoft.AspNetCore.Mvc;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class UpdateProduct : MinimalEndpoint<UpdateProductBody>
{
    private readonly ProductsRepository _products;

    public UpdateProduct(ProductsRepository products)
    {
        _products = products;
    }

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Put("/products/{id}");
    }

    protected override async Task<IResult> Handle(UpdateProductBody req, CancellationToken ct)
    {
        var id = Param<Guid>("id");
        
        var updatedCount = _products.Update(id, req.Name, req.Price);

        await Task.CompletedTask;
        return updatedCount > 0
            ? Results.Ok(_products.Get(id))
            : Results.BadRequest(new { message = "Failed to update" });
    }
}

public record UpdateProductBody(string Name, decimal Price);
