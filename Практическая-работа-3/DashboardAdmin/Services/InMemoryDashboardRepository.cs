using DashboardAdmin.Models;

namespace DashboardAdmin.Services;

public class InMemoryDashboardRepository : IDashboardRepository
{
    // Статический список из 6 карточек: разные тренды, единицы, значения,
    // включая одно отрицательное (Id=5, чистая прибыль).
    private static readonly List<DashboardCard> _cards = new()
    {
        new DashboardCard { Id = 1, Title = "Выручка",        Value = 1250000m, Trend = Trend.Up,     Unit = "руб.", Description = "Общая выручка за период" },
        new DashboardCard { Id = 2, Title = "Заказы",         Value = 842m,     Trend = Trend.Up,     Unit = "шт.",  Description = "Количество оформленных заказов" },
        new DashboardCard { Id = 3, Title = "Возвраты",       Value = 37m,      Trend = Trend.Down,   Unit = "шт.",  Description = "Число возвратов товара" },
        new DashboardCard { Id = 4, Title = "Конверсия",      Value = 3.4m,     Trend = Trend.Stable, Unit = "%",    Description = "Доля посетителей, сделавших заказ" },
        new DashboardCard { Id = 5, Title = "Чистая прибыль", Value = -18500m,  Trend = Trend.Down,   Unit = "руб.", Description = "Убыток по итогам периода" },
        new DashboardCard { Id = 6, Title = "Новые клиенты",  Value = 156m,     Trend = Trend.Up,     Unit = "чел.", Description = "Зарегистрировано новых клиентов" }
    };

    public List<DashboardCard> GetAllCards() => _cards.ToList();

    public DashboardCard? GetById(int id) => _cards.FirstOrDefault(c => c.Id == id);

    public List<DashboardCard> GetLatest(int count) =>
        _cards.OrderByDescending(c => c.Id).Take(count).ToList();
}
