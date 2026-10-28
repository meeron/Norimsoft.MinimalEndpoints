using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Tests.Endpoints;

public enum TestStatus
{
    Active,
    Archived,
}

[Flags]
public enum TestPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
}

public class ParamTypesEndpoint : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/param-types");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        return Task.FromResult(Results.Ok(new ParamTypesResponse(
            Param<DateTime?>("dateTime"),
            Param<DateTimeOffset?>("dateTimeOffset"),
            Param<DateOnly?>("dateOnly"),
            Param<TimeOnly?>("timeOnly"),
            Param<TimeSpan?>("timeSpan"),
            Param<float?>("float"),
            Param<short?>("short"),
            Param<byte?>("byte"),
            Param<TestStatus?>("status")?.ToString(),
            Param<TestPermissions?>("permissions")?.ToString())));
    }
}

public record ParamTypesResponse(
    DateTime? DateTime,
    DateTimeOffset? DateTimeOffset,
    DateOnly? DateOnly,
    TimeOnly? TimeOnly,
    TimeSpan? TimeSpan,
    float? Float,
    short? Short,
    byte? Byte,
    string? Status,
    string? Permissions);

public class UnsupportedParamEndpoint : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/param-unsupported/{method}");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        object? value = Param("method") switch
        {
            "try" => TryParam<Uri>("url", out var uri) ? uri : null,
            "required" => RequiredParam<Uri>("url"),
            _ => Param<Uri>("url"),
        };
        return Task.FromResult(Results.Ok(value));
    }
}

public class TryParamEndpoint : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/try-param");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        var found = TryParam<int>("page", out var page);
        return Task.FromResult(Results.Ok(new TryParamResponse(found, page)));
    }
}

public record TryParamResponse(bool Found, int Page);

public class RequiredParamEndpoint : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/required-param/{id}");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        var id = RequiredParam<Guid>("id");
        var count = RequiredParam<int>("count");
        return Task.FromResult(Results.Ok(new RequiredParamResponse(id, count)));
    }
}

public record RequiredParamResponse(Guid Id, int Count);

public class RequiredStringParamEndpoint : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/required-string-param");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        return Task.FromResult(Results.Ok(RequiredParam<string>("name")));
    }
}
