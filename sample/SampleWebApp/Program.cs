using Norimsoft.MinimalEndpoints;
using SampleWebApp.Books;
using SampleWebApp.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ProductsRepository>();

// Register endpoints from "this" assembly
builder.Services.AddMinimalEndpoints();

// Register endpoints from other assemblies 
builder.Services.AddMinimalEndpoints(c => c.FromAssemblyContaining<Book>());
    //.FromAssemblyContaining<Program>()); Could also be used to register from "this" assembly"

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMinimalEndpoints(config =>
{
    // Use custom error handler
    //config.OnError = (ex, httpContext) => Results.BadRequest(new { ex.Message, Path = httpContext.Request.Path.Value });
});
app.Run();
public partial class Program { }
