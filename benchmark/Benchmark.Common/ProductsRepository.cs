using Bogus;

namespace Benchmark.Common;

public class ProductsRepository
{
    public IReadOnlyList<Product> GetAll(int count)
    {
        return new Faker<Product>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Commerce.Product())
            .RuleFor(x => x.Price, f => f.Finance.Amount(min: 5, max: 1000))
            .Generate(count);
    }
}

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
