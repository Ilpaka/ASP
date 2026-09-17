using ShopFront.Models;

namespace ShopFront.Services;

public class InMemoryProductRepository : IProductRepository
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Беспроводные наушники", Category = "Электроника", Price = 5990,  OldPrice = 8990, Stock = 12, IsHit = true,  Image = "electronics.svg", Description = "Активное шумоподавление, до 30 часов работы." },
        new Product { Id = 2, Name = "Смартфон Nova 12",      Category = "Электроника", Price = 42990, OldPrice = null, Stock = 0,  IsHit = false, Image = "electronics.svg", Description = "AMOLED-экран 120 Гц, тройная камера." },
        new Product { Id = 3, Name = "Умная колонка Mini",    Category = "Электроника", Price = 3490,  OldPrice = 3990, Stock = 25, IsHit = false, Image = "electronics.svg", Description = "Голосовой помощник и мультирум." },
        new Product { Id = 4, Name = "Зимняя куртка Arctic",  Category = "Одежда",      Price = 6990,  OldPrice = 12990, Stock = 7, IsHit = false, Image = "clothes.svg",     Description = "Мембрана 10 000 мм, утеплитель до −25 °C." },
        new Product { Id = 5, Name = "Кроссовки Runner Pro",  Category = "Одежда",      Price = 7490,  OldPrice = 8990, Stock = 18, IsHit = true,  Image = "clothes.svg",     Description = "Амортизация для бега по асфальту." },
        new Product { Id = 6, Name = "Футболка Basic",        Category = "Одежда",      Price = 1290,  OldPrice = null, Stock = 40, IsHit = false, Image = "clothes.svg",     Description = "100% хлопок, плотность 180 г/м²." },
        new Product { Id = 7, Name = "C# в глубину",          Category = "Книги",       Price = 2190,  OldPrice = 2790, Stock = 9,  IsHit = false, Image = "books.svg",       Description = "Продвинутые возможности языка и CLR." },
        new Product { Id = 8, Name = "Чистый код",            Category = "Книги",       Price = 1890,  OldPrice = 2990, Stock = 15, IsHit = true,  Image = "books.svg",       Description = "Практики написания поддерживаемого кода." },
        new Product { Id = 9, Name = "Алгоритмы. Построение", Category = "Книги",       Price = 4590,  OldPrice = null, Stock = 0,  IsHit = false, Image = "books.svg",       Description = "Классический справочник по алгоритмам." }
    };

    public List<Product> GetAll() => _products.ToList();

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public List<string> GetCategories() =>
        _products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
}
