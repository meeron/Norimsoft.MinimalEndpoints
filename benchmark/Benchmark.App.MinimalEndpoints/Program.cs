using Benchmark.Common;
using Norimsoft.MinimalEndpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

builder.Services.AddMinimalEndpoints();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<ProductsRepository>();

var app = builder.Build();

app.UseMinimalEndpoints();
app.Run();
