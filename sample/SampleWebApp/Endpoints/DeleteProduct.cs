using Microsoft.AspNetCore.Mvc;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class DeleteProduct : MinimalEndpoint<DeleteProductReq>
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

    protected override async Task<IResult> Handle(DeleteProductReq req, CancellationToken ct)
    {
        var deletedCount = _products.Delete(req.Id);
        
        await Task.CompletedTask;
        return Ok(new { deletedCount });
    }
}

public class DeleteProductReq
{
    [FromRoute]
    public Guid Id { get; init; }
}
