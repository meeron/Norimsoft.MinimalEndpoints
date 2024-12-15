using System.Net;
using FluentAssertions;
using SampleWebApp.Models;

namespace SampleWebApp.Tests.Tests;

[Collection(TestCollection.Name)]
public class EndpointsTests
{
    private readonly HttpClient _client;

    public EndpointsTests(SampleWebAppTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/products")]
    [InlineData("/products/84F46487-0EE1-4E64-95E4-F4F7243512FB")]
    [InlineData("/books")]
    public async Task Get_ShouldResponse_Ok(string url)
    {
        // Act
        var res = await _client.GetAsync(url);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task GetWithQueryString_ShouldResponse_Ok()
    {
        // Act
        var res = await _client.GetAsync("/products?q=apple");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtops = await res.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        
        dtops.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task GetWithQueryDecimal_ShouldResponse_Ok()
    {
        // Act
        var res = await _client.GetAsync("/products?price=9.5");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var dtops = await res.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        
        dtops.Count().Should().Be(1);
    }
    
    [Fact]
    public async Task Post_ShouldResponse_Created()
    {
        // Arrange
        const string name = "test1";
        const decimal price = 8.99M;
        
        var content = JsonContent.Create(new { name, price });
        
        // Act
        var res = await _client.PostAsync("/products", content);
        
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
        var content = JsonContent.Create(new
        {
            Name = "new name",
        });
        
        // Act
        var res = await _client.PutAsync("/products/D8D8AEA6-3984-492E-9062-EA03B19B0B4F", content);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    

    [Fact]
    public async Task Delete_ShouldResponse_Ok()
    {
        // Act
        var res = await _client.DeleteAsync("/products/110D4653-83C5-40AE-BB54-DD1A661FF334");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Patch_ShouldResponse_Ok()
    {
        // Arrange
        var content = JsonContent.Create(new { });
        
        // Act
        var res = await _client.PatchAsync("/products/patch", content);
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Head_ShouldResponse_Ok()
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri("/products/head"),
            Method = HttpMethod.Head,
        };
        
        // Act
        var res = await _client.SendAsync(request);
        var headerValue = res.Headers.GetValues("X-Test").First();
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
        headerValue.Should().Be("Test");
    }
    
    [Fact]
    public async Task GetWithError_ShouldResponse_Problem()
    {
        // Act
        var res = await _client.GetAsync("/error");
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
        var content = JsonContent.Create(new { });
        
        // Act
        var res = await _client.PostAsync("/books/invalid", content);
        var validationResult = await res.Content.ReadFromJsonAsync<ValidationResult>();
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        validationResult!.Title.Should().Be("One or more validation errors occurred.");
        validationResult.Status.Should().Be(400);
        validationResult.Errors!["Name"][0].Should().Be("'Name' must not be empty.");
    }
}

public record ErrorDetails(string Type, string Title, int Status);
