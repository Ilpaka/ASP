using ShopFront.Models;

namespace ShopFront.Services;

public interface ICartService
{
    CartViewModel Get();
    void Add(Product product);
    void Remove(int productId);
    void Clear();
}
