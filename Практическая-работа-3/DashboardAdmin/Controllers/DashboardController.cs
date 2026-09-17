using DashboardAdmin.Models;
using DashboardAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardAdmin.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardRepository _repository;

    public DashboardController(IDashboardRepository repository)
    {
        _repository = repository;
    }

    // GET /  и  /Dashboard?trend=Up&sort=value_desc&q=текст
    public IActionResult Index(string trend = "all", string sort = "id", string? q = null)
    {
        var all = _repository.GetAllCards();

        // --- Фильтр по тренду (кликабельные чипы) ---
        IEnumerable<DashboardCard> cards = all;
        if (Enum.TryParse<Trend>(trend, ignoreCase: true, out var parsedTrend))
        {
            cards = cards.Where(c => c.Trend == parsedTrend);
        }

        // --- Поиск по заголовку и описанию ---
        if (!string.IsNullOrWhiteSpace(q))
        {
            cards = cards.Where(c =>
                c.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        // --- Сортировка (кликабельные варианты) ---
        cards = sort switch
        {
            "value_desc" => cards.OrderByDescending(c => c.Value),
            "value_asc" => cards.OrderBy(c => c.Value),
            "title" => cards.OrderBy(c => c.Title),
            _ => cards.OrderBy(c => c.Id)
        };

        var result = cards.ToList();

        // Состояние фильтров — во View через ViewBag (нужно для подсветки активных чипов)
        ViewBag.Trend = trend;
        ViewBag.Sort = sort;
        ViewBag.Query = q;

        // Счётчики для чипов — считаются по всем карточкам, а не по отфильтрованным
        ViewBag.CountAll = all.Count;
        ViewBag.CountUp = all.Count(c => c.Trend == Trend.Up);
        ViewBag.CountDown = all.Count(c => c.Trend == Trend.Down);
        ViewBag.CountStable = all.Count(c => c.Trend == Trend.Stable);

        // Задание 6: данные через ViewData / ViewBag
        ViewData["ReportPeriod"] = "Сентябрь 2026";
        ViewBag.Currency = "RUB";

        return View(result); // типизированная модель
    }

    // GET /Dashboard/Details/3 — карточка целиком (использует GetById репозитория)
    public IActionResult Details(int id)
    {
        var card = _repository.GetById(id);
        if (card == null) return NotFound();

        // Для блока «другие показатели» на странице детали
        ViewBag.Latest = _repository.GetLatest(3).Where(c => c.Id != id).ToList();
        ViewData["ReportPeriod"] = "Сентябрь 2026";
        ViewBag.Currency = "RUB";

        return View(card);
    }

    // GET /Dashboard/Print — тот же список, но в режиме печати (доп. уровень «Средний»)
    public IActionResult Print()
    {
        var cards = _repository.GetAllCards();

        ViewData["Mode"] = "Print"; // _ViewStart выберет _PrintLayout
        ViewData["ReportPeriod"] = "Сентябрь 2026";
        ViewBag.Currency = "RUB";

        // Для печати фильтры не применяются — показываем всё
        ViewBag.Trend = "all";
        ViewBag.Sort = "id";
        ViewBag.Query = null;
        ViewBag.CountAll = cards.Count;
        ViewBag.CountUp = cards.Count(c => c.Trend == Trend.Up);
        ViewBag.CountDown = cards.Count(c => c.Trend == Trend.Down);
        ViewBag.CountStable = cards.Count(c => c.Trend == Trend.Stable);

        return View("Index", cards);
    }
}
