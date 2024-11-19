using Norimsoft.MinimalEndpoints;
using SampleWebApp.Books;
using SampleWebApp.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<ProductsRepository>();

// Register endpoints from "this" assembly
builder.Services.AddMinimalEndpoints();

// Register endpoints from other assemblies 
builder.Services.AddMinimalEndpoints(c => c.FromAssemblyContaining<Book>());
    //.FromAssemblyContaining<Program>()); Could also be used to register from "this" assembly"

var app = builder.Build();

app.MapScalarApiReference(); // scalar/v1
app.MapOpenApi();

app.UseMinimalEndpoints(_ =>
{
    // Use custom error handler
    //config.OnError = (ex, httpContext) => Results.BadRequest(new { ex.Message, Path = httpContext.Request.Path.Value });
});
app.Run();
public partial class Program { }
