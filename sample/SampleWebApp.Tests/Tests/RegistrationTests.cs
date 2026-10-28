using System.Net;
using System.Reflection;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Norimsoft.MinimalEndpoints;
using SampleWebApp.Tests.Endpoints;

namespace SampleWebApp.Tests.Tests;

public class RegistrationTests
{
    [Fact]
    public async Task SameAssemblyRegisteredTwice_ShouldMapEachEndpointOnce()
    {
        // Arrange
        await using var app = TestApp.Build(configureServices: services =>
        {
            services.AddMinimalEndpoints(c => c.FromAssemblyContaining<PingEndpoint>());
            services.AddMinimalEndpoints(c => c.FromAssemblyContaining<PingEndpoint>());
        });
        app.UseMinimalEndpoints();
        await app.StartAsync();

        // Act
        var res = await app.GetTestClient().GetAsync("/test/ping");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TwoApps_BuiltBeforeEitherMapsEndpoints_ShouldEachMapTheirOwnEndpoints()
    {
        // Arrange
        await using var first = TestApp.Build();
        await using var second = TestApp.Build();
        first.UseMinimalEndpoints();
        second.UseMinimalEndpoints();
        await first.StartAsync();
        await second.StartAsync();

        // Act
        var firstRes = await first.GetTestClient().GetAsync("/test/ping");
        var secondRes = await second.GetTestClient().GetAsync("/test/ping");

        // Assert
        firstRes.StatusCode.Should().Be(HttpStatusCode.OK);
        secondRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EndpointDerivedFromAbstractBase_ShouldBeMapped()
    {
        // Arrange
        await using var app = TestApp.Build();
        app.UseMinimalEndpoints();
        await app.StartAsync();

        // Act
        var res = await app.GetTestClient().GetAsync("/test/derived");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.Content.ReadFromJsonAsync<string>()).Should().Be("Hello derived");
    }

    [Fact]
    public async Task EndpointWithRequestDerivedFromAbstractGenericBase_ShouldBeMapped()
    {
        // Arrange
        await using var app = TestApp.Build();
        app.UseMinimalEndpoints();
        await app.StartAsync();

        // Act
        var res = await app.GetTestClient().PostAsJsonAsync("/test/derived-request", new EchoRequest("hi"));

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        (await res.Content.ReadFromJsonAsync<EchoRequest>()).Should().Be(new EchoRequest("hi"));
    }

    [Fact]
    public async Task UseMinimalEndpoints_WithoutAddMinimalEndpoints_ShouldNotThrow()
    {
        // Arrange
        await using var app = TestApp.Build(configureServices: _ => { });

        // Act
        var act = () => app.UseMinimalEndpoints();

        // Assert
        act.Should().NotThrow();
        await app.StartAsync();
        (await app.GetTestClient().GetAsync("/test/ping")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssemblyWithTypesThatFailToLoad_ShouldRegisterLoadableEndpointsAndValidators()
    {
        // Arrange
        var assembly = new PartiallyLoadableAssembly(
            typeof(DerivedWithRequestEndpoint),
            typeof(DerivedWithRequestEndpoint.Validator),
            null);
        await using var app = TestApp.Build(configureServices: services =>
            services.AddMinimalEndpoints(c => c.FromAssembly(assembly)));
        app.UseMinimalEndpoints();
        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var validRes = await client.PostAsJsonAsync("/test/derived-request", new EchoRequest("hi"));
        var invalidRes = await client.PostAsJsonAsync("/test/derived-request", new EchoRequest(""));
        var notScannedRes = await client.GetAsync("/test/ping");

        // Assert
        validRes.StatusCode.Should().Be(HttpStatusCode.OK);
        invalidRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        notScannedRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Simulates an assembly where some types cannot be loaded, e.g. because a dependency is missing.
    /// </summary>
    private class PartiallyLoadableAssembly(params Type?[] types) : Assembly
    {
        public override Type[] GetTypes() =>
            throw new ReflectionTypeLoadException(types, [new TypeLoadException("Could not load type")]);

        public override Type[] GetExportedTypes() => GetTypes();
    }
}
