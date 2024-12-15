# Norimsoft.MinimalEndpoints
Small and developer friendly library to easily structure and configure minimal api endpoints.

## Getting started
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
