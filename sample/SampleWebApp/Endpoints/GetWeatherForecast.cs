using Norimsoft.MinimalEndpoints;

namespace SampleWebApp.Endpoints;

public class GetWeatherForecast : MinimalEndpoint
{
    protected override void Configure(EndpointRoute route)
    {
        route.Get("/weatherforecast")
            .Produces<WeatherForecast[]>()
            .WithName("GetWeatherForecast")
            .WithOpenApi();
    }

    protected override async Task<IResult> Handle(CancellationToken ct)
    {
        await Task.CompletedTask;
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();

        return Ok(forecast);
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
