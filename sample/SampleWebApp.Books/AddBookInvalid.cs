using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Books;

public class AddBookInvalid : MinimalEndpoint<NewBook>
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Post("/books/invalid");
    }

    protected override async Task<IResult> Handle(NewBook req, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Results.Ok();
    }

    public class Validator : AbstractValidator<NewBook>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }
}

public record NewBook(string Name);
