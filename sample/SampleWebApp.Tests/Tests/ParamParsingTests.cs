using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Tests.Endpoints;

namespace SampleWebApp.Tests.Tests;

public class ParamParsingTests : IAsyncLifetime
{
    private static readonly Guid Id = Guid.Parse("4A99555A-789D-4C27-917B-BA8C1505A0E9");

    private readonly TestLoggerProvider _logs = new();
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = TestApp.Build(loggerProvider: _logs);
        _app.UseMinimalEndpoints();
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync() => await _app.DisposeAsync();

    [Fact]
    public async Task Param_ShouldParseAdditionalTypes()
    {
        // Act
        var res = await _client.GetFromJsonAsync<ParamTypesResponse>(
            "/test/param-types?dateTime=2026-10-28T13:45:00&dateTimeOffset=2026-10-28T13:45:00%2B02:00" +
            "&dateOnly=2026-10-28&timeOnly=13:45&timeSpan=01:30:00&float=1.5&short=-12&byte=255" +
            "&status=archived&permissions=Read,Write");

        // Assert
        res.Should().Be(new ParamTypesResponse(
            new DateTime(2026, 10, 28, 13, 45, 0),
            new DateTimeOffset(2026, 10, 28, 13, 45, 0, TimeSpan.FromHours(2)),
            new DateOnly(2026, 10, 28),
            new TimeOnly(13, 45),
            TimeSpan.FromMinutes(90),
            1.5f,
            -12,
            255,
            "Archived",
            "Read, Write"));
    }

    [Theory]
    [InlineData("status=Deleted")]
    [InlineData("status=5")]
    [InlineData("byte=256")]
    [InlineData("dateOnly=28.10.2026")]
    public async Task Param_ShouldReturnDefault_WhenValueIsInvalid(string query)
    {
        // Act
        var res = await _client.GetFromJsonAsync<ParamTypesResponse>($"/test/param-types?{query}");

        // Assert
        res.Should().Be(new ParamTypesResponse(null, null, null, null, null, null, null, null, null, null));
    }

    [Theory]
    [InlineData("param", "")]
    [InlineData("param", "?url=https://example.com")]
    [InlineData("try", "?url=https://example.com")]
    [InlineData("required", "?url=https://example.com")]
    public async Task UnsupportedType_ShouldThrow(string method, string query)
    {
        // Act
        var res = await _client.GetAsync($"/test/param-unsupported/{method}{query}");
        var problem = await res.Content.ReadFromJsonAsync<ProblemResponse>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        problem!.Type.Should().Be(typeof(NotSupportedException).FullName);
        problem.Title.Should().Contain("System.Uri");
    }

    [Theory]
    [InlineData("?page=3", true, 3)]
    [InlineData("?page=0", true, 0)]
    [InlineData("?page=-3", true, -3)]
    [InlineData("", false, 0)]
    [InlineData("?page=", false, 0)]
    [InlineData("?page=abc", false, 0)]
    [InlineData("?page=1.5", false, 0)]
    public async Task TryParam_ShouldReportWhetherValueIsPresentAndValid(string query, bool found, int page)
    {
        // Act
        var res = await _client.GetFromJsonAsync<TryParamResponse>($"/test/try-param{query}");

        // Assert
        res.Should().Be(new TryParamResponse(found, page));
    }

    [Fact]
    public async Task RequiredParam_ShouldReturnValues_WhenValid()
    {
        // Act
        var res = await _client.GetAsync($"/test/required-param/{Id}?count=0");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.Content.ReadFromJsonAsync<RequiredParamResponse>()).Should().Be(new RequiredParamResponse(Id, 0));
    }

    [Theory]
    [InlineData("/test/required-param/not-a-guid?count=1", "id", "The value of the 'id' parameter is not valid.")]
    [InlineData("/test/required-param/4A99555A-789D-4C27-917B-BA8C1505A0E9", "count", "The 'count' parameter is required.")]
    [InlineData("/test/required-param/4A99555A-789D-4C27-917B-BA8C1505A0E9?count=", "count", "The 'count' parameter is required.")]
    [InlineData("/test/required-param/4A99555A-789D-4C27-917B-BA8C1505A0E9?count=many", "count", "The value of the 'count' parameter is not valid.")]
    [InlineData("/test/required-string-param", "name", "The 'name' parameter is required.")]
    [InlineData("/test/required-string-param?name=", "name", "The 'name' parameter is required.")]
    public async Task RequiredParam_ShouldRespondBadRequest_WhenMissingOrInvalid(string url, string parameter, string error)
    {
        // Act
        var res = await _client.GetAsync(url);
        var validation = await res.Content.ReadFromJsonAsync<ValidationResult>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        validation!.Status.Should().Be(400);
        validation.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]> { [parameter] = [error] });
        _logs.Entries.Should().NotContain(e => e.Level >= LogLevel.Error);
    }

    [Fact]
    public async Task RequiredParam_ShouldRespondBadRequest_WhenOnErrorIsSet()
    {
        // Arrange
        await using var app = TestApp.Build();
        app.UseMinimalEndpoints(c => c.OnError = (_, _) => Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
        await app.StartAsync();

        // Act
        var res = await app.GetTestClient().GetAsync($"/test/required-param/{Id}");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
