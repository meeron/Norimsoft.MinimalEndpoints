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
        var @int = Param<int>("int");
        var intNull = Param<int?>("intNull");
        var text = Param<string>("text");
        var guid = Param<Guid>("guid");
        var guidNull = Param<Guid?>("guidNull");
        var @bool = Param<bool>("bool");
        var boolNull = Param<bool?>("boolNull");
        var @double = Param<double>("double");
        var doubleNull = Param<double?>("doubleNull");
        var @long = Param<long>("long");
        var longNull = Param<long?>("longNull");
        var @decimal = Param<decimal>("decimal");
        var decimalNull = Param<decimal?>("decimalNull");

        await Task.CompletedTask;
        return Results.Ok(new
        {
            id,
            @int,
            intNull,
            text,
            guid,
            guidNull,
            @bool,
            boolNull,
            @double,
            doubleNull,
            @long,
            longNull,
            @decimal,
            decimalNull,
        });
    }
}
