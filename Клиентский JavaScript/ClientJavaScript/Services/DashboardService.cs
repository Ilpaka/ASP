using ClientJavaScript.Models;

namespace ClientJavaScript.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly object _sync = new();
    private readonly List<Metric> _metrics =
    [
        new(1, "Активные пользователи", 1284, "чел.", "Пользователи за последние 30 дней"),
        new(2, "Заказы сегодня", 86, "шт.", "Оплаченные заказы за сегодня"),
        new(3, "Выручка", 428_750, "₽", "Сумма оплаченных заказов сегодня"),
        new(4, "Средний чек", 4_985, "₽", "Среднее значение оплаченного заказа")
    ];

    private readonly List<Activity> _activities =
    [
        new(1, "Новый заказ №1042", "Продажи", DateTime.Now.AddMinutes(-8)),
        new(2, "Обновлён каталог товаров", "Каталог", DateTime.Now.AddMinutes(-31)),
        new(3, "Добавлен новый клиент", "Клиенты", DateTime.Now.AddHours(-1))
    ];

    private int _nextId = 4;

    public DashboardSnapshot GetSnapshot()
    {
        lock (_sync)
            return new DashboardSnapshot(_metrics.ToArray(), _activities.OrderByDescending(x => x.CreatedAt).ToArray());
    }

    public Activity? AddActivity(string title, string category)
    {
        title = title.Trim();
        category = category.Trim();
        if (title.Length is < 3 or > 80 || category.Length is < 2 or > 30)
            return null;

        lock (_sync)
        {
            var activity = new Activity(_nextId++, title, category, DateTime.Now);
            _activities.Insert(0, activity);
            if (_activities.Count > 20) _activities.RemoveAt(_activities.Count - 1);
            return activity;
        }
    }
}
