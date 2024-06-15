using System.Net;
using FluentAssertions;

namespace SampleWebApp.Tests.Tests;

public class EndpointsTests : IClassFixture<SampleWebAppTestFactory>
{
    private readonly SampleWebAppTestFactory _factory;

    public EndpointsTests(SampleWebAppTestFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/products")]
    [InlineData("/products/84F46487-0EE1-4E64-95E4-F4F7243512FB")]
    [InlineData("/books")]
    public async Task Get_ShouldResponse_Ok(string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var res = await client.GetAsync(url);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Theory]
    [InlineData("/products")]
    public async Task Post_ShouldResponse_Created(string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { });
        
        // Act
        var res = await client.PostAsync(url, content);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Created);
    }
    
    [Theory]
    [InlineData("/products/84F46487-0EE1-4E64-95E4-F4F7243512FB")]
    public async Task Put_ShouldResponse_Ok(string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { });
        
        // Act
        var res = await client.PutAsync(url, content);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Theory]
    [InlineData("/products/110D4653-83C5-40AE-BB54-DD1A661FF334")]
    public async Task Delete_ShouldResponse_Ok(string url)
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var res = await client.DeleteAsync(url);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
