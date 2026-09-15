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

    // GET /  и  /Dashboard
    public IActionResult Index()
    {
        var cards = _repository.GetAllCards();

        // Задание 6: данные через ViewData / ViewBag
        ViewData["ReportPeriod"] = "Сентябрь 2026";
        ViewBag.Currency = "RUB";

        return View(cards); // типизированная модель
    }

    // GET /Dashboard/Print — тот же список, но в режиме печати (доп. уровень «Средний»)
    public IActionResult Print()
    {
        var cards = _repository.GetAllCards();

        ViewData["Mode"] = "Print"; // _ViewStart выберет _PrintLayout
        ViewData["ReportPeriod"] = "Сентябрь 2026";
        ViewBag.Currency = "RUB";

        return View("Index", cards);
    }
}
