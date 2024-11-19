using Benchmark.Common;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ProductsRepository>();

var app = builder.Build();

app.MapGet("/products", (int? count, ProductsRepository products) => products.GetAll(count ?? 10));

app.Run();
