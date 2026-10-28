using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Tests.Endpoints;

// Endpoints used by tests that host their own app, see TestApp

public class ThrowingEndpoint : MinimalEndpoint
{
    public const string ErrorMessage = "Connection to db-internal-01 failed";

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/throw");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        throw new InvalidOperationException(ErrorMessage);
    }
}

public class PingEndpoint : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/ping");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        return Task.FromResult(Results.Ok("pong"));
    }
}

public abstract class GreetingEndpointBase : MinimalEndpoint
{
    protected IResult Greet(string name) => Results.Ok($"Hello {name}");
}

public class DerivedEndpoint : GreetingEndpointBase
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/test/derived");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        return Task.FromResult(Greet("derived"));
    }
}

public abstract class EchoEndpointBase<TRequest> : MinimalEndpoint<TRequest>
    where TRequest : class
{
    protected override Task<IResult> Handle(TRequest req, CancellationToken ct)
    {
        return Task.FromResult(Results.Ok(req));
    }
}

public class DerivedWithRequestEndpoint : EchoEndpointBase<EchoRequest>
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Post("/test/derived-request");
    }

    public class Validator : AbstractValidator<EchoRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Text).NotEmpty();
        }
    }
}

public record EchoRequest(string Text);
