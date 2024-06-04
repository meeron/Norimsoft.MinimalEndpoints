using SampleWebApp.Models;

namespace SampleWebApp.Repositories;

public class ProductsRepository
{
    private readonly Dictionary<Guid, Product> _data = new Dictionary<Guid, Product>
    {
        { Guid.Parse("84F46487-0EE1-4E64-95E4-F4F7243512FB"), new Product { Id = Guid.Parse("84F46487-0EE1-4E64-95E4-F4F7243512FB"), Name = "Orange", Price = 9.99M } },
        { Guid.Parse("110D4653-83C5-40AE-BB54-DD1A661FF334"), new Product { Id = Guid.Parse("110D4653-83C5-40AE-BB54-DD1A661FF334"), Name = "Apple", Price = 6.59M } },
    };

    public IEnumerable<Product> GetAll() => _data.Values;

    public Product? Get(Guid id) => _data.GetValueOrDefault(id);

    public void Add(Product newProduct)
    {
        _data.Add(newProduct.Id, newProduct);
    }

    public int Update(Guid id, string name, decimal price)
    {
        if (!_data.ContainsKey(id))
        {
            return 0;
        }

        _data[id] = new Product
        {
            Id = id,
            Name = name,
            Price = price,
        };

        return 1;
    }

    public int Delete(Guid id)
    {
        if (!_data.ContainsKey(id))
        {
            return 0;
        }

        _data.Remove(id);

        return 1;
    }
}
