using CatalogAjax.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAjax.Controllers;

public class AdminController : Controller
{
    private readonly IProductRepository _repo;

    public AdminController(IProductRepository repo)
    {
        _repo = repo;
    }

    public IActionResult Index()
    {
        var all = _repo.GetAll();

        ViewBag.Total = all.Count;
        ViewBag.OutOfStock = all.Count(p => !p.InStock);
        ViewBag.Hits = all.Count(p => p.IsHit);
        ViewBag.BigDiscounts = all.Count(p => p.DiscountPercent > 30);
        ViewBag.StockValue = all.Sum(p => p.Price * p.Stock);
        ViewBag.CartCount = SessionCart.Count(HttpContext.Session);

        return View(all);
    }
}
