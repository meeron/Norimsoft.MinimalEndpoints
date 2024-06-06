using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Books;

public class GetBooks : MinimalEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/books");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        var books = new[]
        {
            new Book("Lord of The Rings", "J.R.R. Tolkien"),
            new Book("Dune", "Frank Herbert"),
        };

        return Task.FromResult(Ok(books));
    }
}

public record Book(string Title, string Author);
