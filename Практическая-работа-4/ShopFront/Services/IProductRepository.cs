using ShopFront.Models;

namespace ShopFront.Services;

public interface IProductRepository
{
    List<Product> GetAll();
    Product? GetById(int id);
    List<string> GetCategories();
}
