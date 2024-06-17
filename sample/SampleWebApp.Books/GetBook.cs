using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Books;

public class GetBook : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/books/{id}");
    }

    protected override async Task<IResult> Handle(CancellationToken ct)
    {
        var id = Param<int>("id");
        var text = Param<string>("text");
        var textFallback = Param("textFallback", fallback: "n/a");
        var guid = Param<Guid>("guid");
        var boolean = Param<bool>("bool");
        var @double = Param<double>("double");

        await Task.CompletedTask;
        return Results.Ok(new { id, text, guid, boolean, @double, textFallback });
    }
}
