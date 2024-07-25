namespace Benchmark.App;

using BenchmarkDotNet.Attributes;

public class MinimalEndpointsVsMinimalApi
{
    private const int Count = 100;
    
    private readonly HttpClient _minimalEndpointsClient;
    private readonly HttpClient _minimalApiClient;

    public MinimalEndpointsVsMinimalApi()
    {
        _minimalEndpointsClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:6060"),
        };
        _minimalApiClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:7070"),
        };
    }
    
    [Benchmark]
    public async Task GetProducts_MinimalEndpoints()
    {
        var res = await _minimalEndpointsClient.GetAsync($"products?count={Count}");
        res.EnsureSuccessStatusCode();
    }
    
    [Benchmark]
    public async Task GetProducts_MinimalApi()
    {
        var res = await _minimalApiClient.GetAsync($"products?count={Count}");
        res.EnsureSuccessStatusCode();
    }
}
