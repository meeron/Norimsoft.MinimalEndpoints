namespace SampleWebApp.Models;

public class Product
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public decimal Price { get; init; }
}
