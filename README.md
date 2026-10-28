# Norimsoft.MinimalEndpoints
[![Build Norimsoft.MinimalEndpoints](https://github.com/meeron/Norimsoft.MinimalEndpoints/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/meeron/Norimsoft.MinimalEndpoints/actions/workflows/ci.yml)

Small and developer friendly library to easily structure and configure minimal api endpoints.

## Getting started
### Installation
Install from [NuGet](https://www.nuget.org/packages/Norimsoft.MinimalEndpoints)

or
```shell
dotnet add package Norimsoft.MinimalEndpoints
```

### Program.cs
```cs
using Norimsoft.MinimalEndpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMinimalEndpoints();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMinimalEndpoints();
app.Run();
```
### GET Endpoint
```cs
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
        // Get id from route param
        var productId = Param<Guid>("id");
        
        // Get query string parameter
        var version = Param<int?>("version");
        
        var product = await _products.Get(productId);
        
        return product != null ? Results.Ok(product) : Results.NoContent();
    }
}
```
### POST Endpoint with request
```cs
public class AddProduct : MinimalEndpoint<NewProduct>
{
    private readonly ProductsRepository _products;

    public AddProduct(ProductsRepository products)
    {
        _products = products;
    }

    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Post("/products");
    }

    protected override async Task<IResult> Handle(NewProduct req, CancellationToken ct)
    {
        var id = Guid.NewGuid();

        var newProduct = new Product
        {
            Id = id,
            Name = req.Name,
            Price = req.Price,
        };
        
        await _products.Add(newProduct);
        
        return Results.Created($"/products/{id}", newProduct);
    }
}

public record NewProduct(string Name, decimal Price);
```
### FluentValidation integration
Just add a validator as you usually do using [FluentValidation](https://docs.fluentvalidation.net/en/latest/start.html)
```cs
public class AddBook : MinimalEndpoint<NewBook>
{
    private readonly IBooksReppository _books;
    
    public AddBook(IBooksReppository books)
    {
        _books = books;
    }
    
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Post("/books");
    }

    protected override async Task<IResult> Handle(NewBook req, CancellationToken ct)
    {
        await _books.Add(new Book { Name = req.Name });
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
```

### Shared base endpoints
Endpoints can derive from your own abstract base classes, e.g. to share helpers. Only concrete classes are registered as endpoints.
```cs
public abstract class ProductsEndpoint : MinimalEndpoint
{
    protected IResult NotFoundProduct(Guid id) =>
        Results.Problem(statusCode: 404, title: $"Product {id} not found");
}

public class GetProduct : ProductsEndpoint
{
    protected override RouteHandlerBuilder Configure(EndpointRoute route)
    {
        return route.Get("/products/{id}");
    }

    protected override Task<IResult> Handle(CancellationToken ct)
    {
        var id = Param<Guid>("id");
        return Task.FromResult(NotFoundProduct(id));
    }
}
```

### Endpoints from other assemblies
`AddMinimalEndpoints()` registers endpoints and validators from the calling assembly. Call it again to add other assemblies; each endpoint is registered only once.
```cs
builder.Services.AddMinimalEndpoints();
builder.Services.AddMinimalEndpoints(c => c.FromAssemblyContaining<Book>());
```

### Error handling
Unhandled exceptions thrown by an endpoint are logged and turned into a `500` problem details response.
The exception type, message and stack trace are included in the response only when the environment is `Development`.

Use `OnError` to return your own response instead:
```cs
app.UseMinimalEndpoints(config =>
{
    config.OnError = (ex, httpContext) => Results.Problem(
        statusCode: 500,
        title: "Something went wrong",
        instance: httpContext.Request.Path);
});
```

#### Check sample project for more examples
