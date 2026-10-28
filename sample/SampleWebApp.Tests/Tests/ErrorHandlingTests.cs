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

public class ErrorHandlingTests
{
    [Fact]
    public async Task Production_ShouldNotExposeExceptionDetails()
    {
        // Arrange
        await using var app = TestApp.Build(environment: "Production");
        app.UseMinimalEndpoints();
        await app.StartAsync();

        // Act
        var res = await app.GetTestClient().GetAsync("/test/throw");
        var body = await res.Content.ReadAsStringAsync();
        var problem = await res.Content.ReadFromJsonAsync<ProblemResponse>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        problem!.Status.Should().Be(500);
        problem.Detail.Should().BeNull();
        body.Should().NotContain(ThrowingEndpoint.ErrorMessage);
        body.Should().NotContain(nameof(InvalidOperationException));
        body.Should().NotContain(nameof(ThrowingEndpoint));
    }

    [Fact]
    public async Task Development_ShouldExposeExceptionDetails()
    {
        // Arrange
        await using var app = TestApp.Build(environment: "Development");
        app.UseMinimalEndpoints();
        await app.StartAsync();

        // Act
        var res = await app.GetTestClient().GetAsync("/test/throw");
        var problem = await res.Content.ReadFromJsonAsync<ProblemResponse>();

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        problem!.Type.Should().Be(typeof(InvalidOperationException).FullName);
        problem.Title.Should().Be(ThrowingEndpoint.ErrorMessage);
        problem.Detail.Should().Contain(nameof(ThrowingEndpoint));
    }

    [Fact]
    public async Task UnhandledException_ShouldBeLogged()
    {
        // Arrange
        var logs = new TestLoggerProvider();
        await using var app = TestApp.Build(environment: "Production", loggerProvider: logs);
        app.UseMinimalEndpoints();
        await app.StartAsync();

        // Act
        await app.GetTestClient().GetAsync("/test/throw");

        // Assert
        logs.Entries.Should().ContainSingle(e => e.Level == LogLevel.Error)
            .Which.Should().Match<LogEntry>(e =>
                e.Category == typeof(ThrowingEndpoint).FullName &&
                e.Exception is InvalidOperationException &&
                e.Message == "Unhandled exception while processing GET /test/throw");
    }

    [Fact]
    public async Task CustomOnError_ShouldBeUsed_AndExceptionStillLogged()
    {
        // Arrange
        var logs = new TestLoggerProvider();
        await using var app = TestApp.Build(loggerProvider: logs);
        app.UseMinimalEndpoints(c => c.OnError = (_, _) => Results.StatusCode(StatusCodes.Status503ServiceUnavailable));
        await app.StartAsync();

        // Act
        var res = await app.GetTestClient().GetAsync("/test/throw");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        logs.Entries.Should().ContainSingle(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException);
    }
}

public record ProblemResponse(string? Type, string? Title, int Status, string? Detail);
