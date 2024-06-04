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
}
