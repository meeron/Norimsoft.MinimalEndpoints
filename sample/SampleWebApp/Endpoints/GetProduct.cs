using Microsoft.AspNetCore.Mvc;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class GetProduct : MinimalEndpoint<GetProductReq>
{
    private readonly ProductsRepository _products;

    public GetProduct(ProductsRepository products)
    {
        _products = products;
    }

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/products/{id}");
    }

    protected override async Task<IResult> Handle(GetProductReq req, CancellationToken ct)
    {
        var product = _products.Get(req.Id);

        await Task.CompletedTask;
        return product != null ? Results.Ok(product) : Results.NoContent();
    }
}

public class GetProductReq
{
    [FromRoute]
    public Guid Id { get; init; }
}
