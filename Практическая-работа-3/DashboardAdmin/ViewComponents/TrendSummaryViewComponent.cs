using DashboardAdmin.Models;
using DashboardAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAdmin.ViewComponents;

// ViewComponent "Сводка по трендам": получает репозиторий через DI,
// группирует карточки по тренду и считает количество и сумму значений.
public class TrendSummaryViewComponent : ViewComponent
{
    private readonly IDashboardRepository _repository;

    public TrendSummaryViewComponent(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public IViewComponentResult Invoke()
    {
        var cards = _repository.GetAllCards();

        var summaries = cards
            .GroupBy(c => c.Trend)
            .Select(g => new TrendSummaryItem
            {
                Trend = g.Key,
                Count = g.Count(),
                Sum = g.Sum(c => c.Value)
            })
            .OrderBy(s => s.Trend)
            .ToList();

        return View(summaries);
    }
}
