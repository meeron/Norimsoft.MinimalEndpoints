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

    protected override void Configure(EndpointRoute route)
    {
        route.Get("/products/{id}");
    }

    protected override async Task<IResult> Handle(GetProductReq req, CancellationToken ct)
    {
        await Task.CompletedTask;

        var product = _products.Get(req.Id);

        return product != null ? Ok(product) : NoContent();
    }
}

public class GetProductReq
{
    [FromRoute]
    public Guid Id { get; init; }
}
