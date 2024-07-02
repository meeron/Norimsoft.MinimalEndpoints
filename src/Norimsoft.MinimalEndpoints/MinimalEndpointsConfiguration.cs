using Microsoft.AspNetCore.Http;

namespace Norimsoft.MinimalEndpoints;

public class MinimalEndpointsConfiguration
{
    internal MinimalEndpointsConfiguration()
    {
    }

    public Func<Exception, HttpContext, IResult>? OnError { get; set; }
}
