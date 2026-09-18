using CatalogAjax.Models;

namespace CatalogAjax.Services;

public class InMemoryProductRepository : IProductRepository
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Беспроводные наушники Air", Category = "Электроника", Price = 5990, OldPrice = 8990, Stock = 12, IsHit = true, Image = "electronics.svg", Description = "Активное шумоподавление, до 30 часов работы." },
        new Product { Id = 2, Name = "Смартфон Nova 12", Category = "Электроника", Price = 42990, OldPrice = null, Stock = 0, IsHit = false, Image = "electronics.svg", Description = "AMOLED-экран 120 Гц, тройная камера." },
        new Product { Id = 3, Name = "Умная колонка Mini", Category = "Электроника", Price = 3490, OldPrice = 3990, Stock = 25, IsHit = false, Image = "electronics.svg", Description = "Голосовой помощник и мультирум." },
        new Product { Id = 4, Name = "Ноутбук Vector 14", Category = "Электроника", Price = 78990, OldPrice = 94990, Stock = 5, IsHit = true, Image = "electronics.svg", Description = "14 дюймов, 16 ГБ ОЗУ, SSD 512 ГБ." },
        new Product { Id = 5, Name = "Механическая клавиатура", Category = "Электроника", Price = 6490, OldPrice = 7990, Stock = 18, IsHit = false, Image = "electronics.svg", Description = "Горячая замена свитчей, RGB-подсветка." },
        new Product { Id = 6, Name = "Монитор 27 дюймов QHD", Category = "Электроника", Price = 24990, OldPrice = null, Stock = 9, IsHit = false, Image = "electronics.svg", Description = "IPS-матрица, 165 Гц, 1 мс." },
        new Product { Id = 7, Name = "Веб-камера Stream HD", Category = "Электроника", Price = 4290, OldPrice = 5990, Stock = 0, IsHit = false, Image = "electronics.svg", Description = "1080p 60 к/с, автофокус." },
        new Product { Id = 8, Name = "Портативная зарядка 20000", Category = "Электроника", Price = 2790, OldPrice = 3490, Stock = 34, IsHit = false, Image = "electronics.svg", Description = "Быстрая зарядка Power Delivery 65 Вт." },
        new Product { Id = 9, Name = "Зимняя куртка Arctic", Category = "Одежда", Price = 6990, OldPrice = 12990, Stock = 7, IsHit = false, Image = "clothes.svg", Description = "Мембрана 10 000 мм, утеплитель до −25 °C." },
        new Product { Id = 10, Name = "Кроссовки Runner Pro", Category = "Одежда", Price = 7490, OldPrice = 8990, Stock = 18, IsHit = true, Image = "clothes.svg", Description = "Амортизация для бега по асфальту." },
        new Product { Id = 11, Name = "Футболка Basic", Category = "Одежда", Price = 1290, OldPrice = null, Stock = 40, IsHit = false, Image = "clothes.svg", Description = "100% хлопок, плотность 180 г/м²." },
        new Product { Id = 12, Name = "Джинсы Slim Fit", Category = "Одежда", Price = 4590, OldPrice = 5990, Stock = 22, IsHit = false, Image = "clothes.svg", Description = "Эластичный деним, прямой крой." },
        new Product { Id = 13, Name = "Худи Oversize", Category = "Одежда", Price = 3890, OldPrice = 5490, Stock = 15, IsHit = true, Image = "clothes.svg", Description = "Футер с начёсом, свободный силуэт." },
        new Product { Id = 14, Name = "Шапка вязаная", Category = "Одежда", Price = 990, OldPrice = 1490, Stock = 0, IsHit = false, Image = "clothes.svg", Description = "Мериносовая шерсть, двойная вязка." },
        new Product { Id = 15, Name = "Рюкзак City 20L", Category = "Одежда", Price = 3290, OldPrice = null, Stock = 27, IsHit = false, Image = "clothes.svg", Description = "Отделение для ноутбука 15 дюймов." },
        new Product { Id = 16, Name = "Кеды Canvas", Category = "Одежда", Price = 2990, OldPrice = 4490, Stock = 11, IsHit = false, Image = "clothes.svg", Description = "Классические парусиновые кеды." },
        new Product { Id = 17, Name = "C# в глубину", Category = "Книги", Price = 2190, OldPrice = 2790, Stock = 9, IsHit = false, Image = "books.svg", Description = "Продвинутые возможности языка и CLR." },
        new Product { Id = 18, Name = "Чистый код", Category = "Книги", Price = 1890, OldPrice = 2990, Stock = 15, IsHit = true, Image = "books.svg", Description = "Практики написания поддерживаемого кода." },
        new Product { Id = 19, Name = "Алгоритмы. Построение", Category = "Книги", Price = 4590, OldPrice = null, Stock = 0, IsHit = false, Image = "books.svg", Description = "Классический справочник по алгоритмам." },
        new Product { Id = 20, Name = "Паттерны проектирования", Category = "Книги", Price = 2690, OldPrice = 3490, Stock = 13, IsHit = false, Image = "books.svg", Description = "Band of Four: 23 классических паттерна." },
        new Product { Id = 21, Name = "Рефакторинг", Category = "Книги", Price = 2390, OldPrice = 2990, Stock = 8, IsHit = false, Image = "books.svg", Description = "Улучшение существующего кода." },
        new Product { Id = 22, Name = "Грокаем алгоритмы", Category = "Книги", Price = 1490, OldPrice = 2190, Stock = 31, IsHit = true, Image = "books.svg", Description = "Иллюстрированное введение в алгоритмы." },
        new Product { Id = 23, Name = "SQL для аналитика", Category = "Книги", Price = 1990, OldPrice = null, Stock = 17, IsHit = false, Image = "books.svg", Description = "Запросы, оконные функции, оптимизация." },
        new Product { Id = 24, Name = "Архитектура ПО", Category = "Книги", Price = 3290, OldPrice = 4790, Stock = 6, IsHit = false, Image = "books.svg", Description = "Чистая архитектура и границы модулей." }
    };

    private static readonly List<Review> _reviews = new()
    {
        new Review { ProductId = 1, Author = "Иван", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 5 },
        new Review { ProductId = 1, Author = "Алексей", Text = "Хорошее качество за свои деньги.", Rating = 3 },
        new Review { ProductId = 1, Author = "Мария", Text = "Отличная вещь, рекомендую.", Rating = 3 },
        new Review { ProductId = 2, Author = "Екатерина", Text = "Ожидал чуть большего, но в целом нормально.", Rating = 3 },
        new Review { ProductId = 2, Author = "Дмитрий", Text = "Пользуюсь месяц — нареканий нет.", Rating = 3 },
        new Review { ProductId = 2, Author = "Иван", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 3 },
        new Review { ProductId = 3, Author = "Дмитрий", Text = "Ожидал чуть большего, но в целом нормально.", Rating = 3 },
        new Review { ProductId = 5, Author = "Мария", Text = "Отличная вещь, рекомендую.", Rating = 5 },
        new Review { ProductId = 5, Author = "Екатерина", Text = "Ожидал чуть большего, но в целом нормально.", Rating = 4 },
        new Review { ProductId = 5, Author = "Мария", Text = "Пользуюсь месяц — нареканий нет.", Rating = 5 },
        new Review { ProductId = 6, Author = "Сергей", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 3 },
        new Review { ProductId = 6, Author = "Екатерина", Text = "Пользуюсь месяц — нареканий нет.", Rating = 4 },
        new Review { ProductId = 7, Author = "Мария", Text = "Хорошее качество за свои деньги.", Rating = 4 },
        new Review { ProductId = 7, Author = "Иван", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 4 },
        new Review { ProductId = 9, Author = "Алексей", Text = "Пришло быстро, упаковка целая.", Rating = 5 },
        new Review { ProductId = 10, Author = "Сергей", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 5 },
        new Review { ProductId = 10, Author = "Ольга", Text = "Ожидал чуть большего, но в целом нормально.", Rating = 3 },
        new Review { ProductId = 11, Author = "Иван", Text = "Ожидал чуть большего, но в целом нормально.", Rating = 4 },
        new Review { ProductId = 11, Author = "Сергей", Text = "Отличная вещь, рекомендую.", Rating = 5 },
        new Review { ProductId = 13, Author = "Дмитрий", Text = "Хорошее качество за свои деньги.", Rating = 5 },
        new Review { ProductId = 13, Author = "Иван", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 5 },
        new Review { ProductId = 14, Author = "Сергей", Text = "Пришло быстро, упаковка целая.", Rating = 3 },
        new Review { ProductId = 15, Author = "Сергей", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 4 },
        new Review { ProductId = 17, Author = "Ольга", Text = "Отличная вещь, рекомендую.", Rating = 4 },
        new Review { ProductId = 17, Author = "Мария", Text = "Пришло быстро, упаковка целая.", Rating = 4 },
        new Review { ProductId = 18, Author = "Екатерина", Text = "Пришло быстро, упаковка целая.", Rating = 5 },
        new Review { ProductId = 19, Author = "Екатерина", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 5 },
        new Review { ProductId = 19, Author = "Екатерина", Text = "Хорошее качество за свои деньги.", Rating = 5 },
        new Review { ProductId = 19, Author = "Екатерина", Text = "Хорошее качество за свои деньги.", Rating = 3 },
        new Review { ProductId = 21, Author = "Ольга", Text = "Пришло быстро, упаковка целая.", Rating = 5 },
        new Review { ProductId = 21, Author = "Екатерина", Text = "Ожидал чуть большего, но в целом нормально.", Rating = 3 },
        new Review { ProductId = 22, Author = "Алексей", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 3 },
        new Review { ProductId = 22, Author = "Сергей", Text = "Полностью соответствует описанию, доволен покупкой.", Rating = 4 },
        new Review { ProductId = 22, Author = "Ольга", Text = "Пришло быстро, упаковка целая.", Rating = 3 },
        new Review { ProductId = 23, Author = "Дмитрий", Text = "Отличная вещь, рекомендую.", Rating = 4 }
    };

    public List<Product> GetAll() => _products.ToList();

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public List<string> GetCategories() =>
        _products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

    // Порция товаров для бесконечной подгрузки (page начинается с 1)
    public List<Product> GetPage(int page, int pageSize, string tag = "all")
    {
        if (page < 1) page = 1;

        var query = _products.AsEnumerable();
        if (tag != "all")
            query = query.Where(p => p.Category == tag);

        return query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    // Поиск по названию и описанию (регистронезависимый)
    public List<Product> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _products.ToList();

        query = query.Trim();
        return _products
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<Review> GetReviews(int productId) =>
        _reviews.Where(r => r.ProductId == productId).ToList();
}
