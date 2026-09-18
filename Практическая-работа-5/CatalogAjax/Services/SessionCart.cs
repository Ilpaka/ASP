using CatalogAjax.Models;

namespace CatalogAjax.Services;

// Корзина хранится в сессии как строка с ID товаров через запятую: "1,3,3,7".
// В отличие от singleton-корзины, у каждого посетителя она своя.
public static class SessionCart
{
    private const string Key = "Cart";

    public static List<int> GetIds(ISession session)
    {
        var raw = session.GetString(Key);
        return string.IsNullOrEmpty(raw)
            ? new List<int>()
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
    }

    public static void SaveIds(ISession session, List<int> ids) =>
        session.SetString(Key, string.Join(',', ids));

    public static int Count(ISession session) => GetIds(session).Count;

    public static void Add(ISession session, int productId)
    {
        var ids = GetIds(session);
        ids.Add(productId);
        SaveIds(session, ids);
    }

    // Убирает одну единицу товара
    public static void RemoveOne(ISession session, int productId)
    {
        var ids = GetIds(session);
        ids.Remove(productId);
        SaveIds(session, ids);
    }

    public static void Clear(ISession session) => session.Remove(Key);

    // Собирает корзину для отображения: группируем ID и подтягиваем товары
    public static CartViewModel Build(ISession session, IProductRepository repo)
    {
        var ids = GetIds(session);

        var items = ids
            .GroupBy(id => id)
            .Select(g =>
            {
                var product = repo.GetById(g.Key);
                if (product == null) return null;

                return new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = g.Count()
                };
            })
            .Where(i => i != null)
            .Select(i => i!)
            .ToList();

        return new CartViewModel { Items = items };
    }
}
