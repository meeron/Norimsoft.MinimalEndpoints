using System.Net;
using FluentAssertions;

namespace SampleWebApp.Tests.Tests;

[Collection(TestCollection.Name)]
public class ParametersBindTests
{
    private readonly HttpClient _client;

    public ParametersBindTests(SampleWebAppTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("some text", "some text")]
    public async Task BindString(string? value, string? expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?text={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.Text.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("not_bool", false)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("FALSE", false)]
    public async Task BindBool(string? value, bool expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?bool={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.Bool.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("not_bool", null)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("FALSE", false)]
    public async Task BindBoolNull(string? value, bool? expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?boolNull={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.BoolNull.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("not_int", 0)]
    [InlineData("123", 123)]
    [InlineData("-123", -123)]
    [InlineData("1.23", 0)]
    public async Task BindInt(string? value, int expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?int={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.Int.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("not_int", null)]
    [InlineData("123", 123)]
    [InlineData("-123", -123)]
    [InlineData("1.23", null)]
    public async Task BindIntNull(string? value, int? expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?intNull={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.IntNull.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, "00000000-0000-0000-0000-000000000000")]
    [InlineData("", "00000000-0000-0000-0000-000000000000")]
    [InlineData("not_guid", "00000000-0000-0000-0000-000000000000")]
    [InlineData("4A99555A-789D-4C27-917B-BA8C1505A0E9", "4A99555A-789D-4C27-917B-BA8C1505A0E9")]
    public async Task BindGuid(string? value, string expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?guid={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.Guid.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("not_guid", null)]
    [InlineData("B53AD0AB-14C2-4266-A41B-282B8B9AC0DD", "B53AD0AB-14C2-4266-A41B-282B8B9AC0DD")]
    public async Task BindGuidNull(string? value, string? expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?guidNull={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        Guid? expectedGuid = expected != null
            ? Guid.Parse(expected)
            : null;
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.GuidNull.Should().Be(expectedGuid);
    }
    
    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("not_long", 0)]
    [InlineData("2947483647", 2947483647)]
    [InlineData("-2947483647", -2947483647)]
    [InlineData("1.23", 0)]
    public async Task BindLong(string? value, long expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?long={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.Long.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("not_long", null)]
    [InlineData("29947483647", 29947483647)]
    [InlineData("-29947483647", -29947483647)]
    [InlineData("1.23", null)]
    public async Task BindLongNull(string? value, long? expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?longNull={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.LongNull.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, 0.0)]
    [InlineData("", 0.0)]
    [InlineData("not_double", 0.0)]
    [InlineData("123", 123)]
    [InlineData("-123", -123)]
    [InlineData("1.23", 1.23)]
    [InlineData("-1.23", -1.23)]
    public async Task BindDouble(string? value, double expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?double={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.Double.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("not_double", null)]
    [InlineData("123", 123D)]
    [InlineData("-123", -123D)]
    [InlineData("1.23", 1.23)]
    [InlineData("-1.23", -1.23)]
    public async Task BindDoubleNull(string? value, double? expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?doubleNull={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.DoubleNull.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, 0.0)]
    [InlineData("", 0.0)]
    [InlineData("not_decimal", 0.0)]
    [InlineData("123", 123)]
    [InlineData("-123", -123)]
    [InlineData("1.23", 1.23)]
    [InlineData("-1.23", -1.23)]
    public async Task BindDecimal(string? value, decimal expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?decimal={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.Decimal.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("not_decimal", null)]
    [InlineData("123", 123D)]
    [InlineData("-123", -123D)]
    [InlineData("1.23", 1.23D)]
    [InlineData("-1.23", -1.23D)]
    public async Task BindDecimalNull(string? value, double? expected)
    {
        // Act
        var res = value == null
            ? await _client.GetAsync("/books/1")
            : await _client.GetAsync($"/books/1?decimalNull={value}");
        
        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dto = await res.Content.ReadFromJsonAsync<BindTest>();
        dto!.DecimalNull.Should().Be((decimal?)expected);
    }
}

public record BindTest(
    string? Text,
    bool Bool,
    bool? BoolNull,
    int Int,
    int? IntNull,
    Guid Guid,
    Guid? GuidNull,
    long Long,
    long? LongNull,
    double Double,
    double? DoubleNull,
    decimal Decimal,
    decimal? DecimalNull);
