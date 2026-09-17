using ShopFront.Models;

namespace ShopFront.Services;

// Корзина живёт в памяти приложения (демо-режим, один покупатель).
public class InMemoryCartService : ICartService
{
    private readonly List<CartItem> _items = new();
    private readonly object _lock = new();

    public CartViewModel Get()
    {
        lock (_lock)
        {
            return new CartViewModel
            {
                Items = _items.Select(i => new CartItem
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }

    public void Add(Product product)
    {
        lock (_lock)
        {
            var existing = _items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existing != null)
            {
                existing.Quantity++;
                return;
            }

            _items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }
    }

    public void Remove(int productId)
    {
        lock (_lock)
        {
            var item = _items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return;

            if (item.Quantity > 1) item.Quantity--;
            else _items.Remove(item);
        }
    }

    public void Clear()
    {
        lock (_lock) { _items.Clear(); }
    }
}
