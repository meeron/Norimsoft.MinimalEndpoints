using System.Net;
using FluentAssertions;
using SampleWebApp.Models;

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
    
    [Fact]
    public async Task Post_ShouldResponse_Created()
    {
        // Arrange
        const string name = "test1";
        const decimal price = 9.99M;
        
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { name, price });
        
        // Act
        var res = await client.PostAsync("/products", content);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var product = await res.Content.ReadFromJsonAsync<Product>();
        product!.Name.Should().Be(name);
        product!.Price.Should().Be(price);
    }

    [Fact]
    public async Task Put_ShouldResponse_Ok()
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { });
        
        // Act
        var res = await client.PutAsync("/products/84F46487-0EE1-4E64-95E4-F4F7243512FB", content);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    

    [Fact]
    public async Task Delete_ShouldResponse_Ok()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var res = await client.DeleteAsync("/products/110D4653-83C5-40AE-BB54-DD1A661FF334");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Patch_ShouldResponse_Ok()
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { });
        
        // Act
        var res = await client.PatchAsync("/products/patch", content);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Head_ShouldResponse_Ok()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri("/products/head"),
            Method = HttpMethod.Head,
        };
        
        // Act
        var res = await client.SendAsync(request);
        var headerValue = res.Headers.GetValues("X-Test").First();
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        headerValue.Should().Be("Test");
    }
    
    [Fact]
    public async Task GetWithError_ShouldResponse_Problem()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var res = await client.GetAsync("/error");
        var errorDetails = await res.Content.ReadFromJsonAsync<ErrorDetails>();
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        errorDetails!.Type.Should().Be("System.Exception");
        errorDetails.Title.Should().Be("Test error");
        errorDetails.Status.Should().Be(500);
    }
    
    [Fact]
    public async Task InvalidPost_ShouldResponse_BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = JsonContent.Create(new { });
        
        // Act
        var res = await client.PostAsync("/books/invalid", content);
        var validationResult = await res.Content.ReadFromJsonAsync<ValidationResult>();
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        validationResult!.Title.Should().Be("One or more validation errors occurred.");
        validationResult.Status.Should().Be(400);
        validationResult.Errors!["Name"][0].Should().Be("'Name' must not be empty.");
    }
}

public record ErrorDetails(string Type, string Title, int Status);
