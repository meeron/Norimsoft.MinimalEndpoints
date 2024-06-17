using Microsoft.AspNetCore.Mvc;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Repositories;

namespace SampleWebApp.Endpoints;

public class GetProduct : MinimalEndpoint
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

    protected override async Task<IResult> Handle(CancellationToken ct)
    {
        var product = _products.Get(Param<Guid>("id"));

        await Task.CompletedTask;
        return product != null ? Results.Ok(product) : Results.NoContent();
    }
}
