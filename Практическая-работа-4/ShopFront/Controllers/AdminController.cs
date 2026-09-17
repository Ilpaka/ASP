using Microsoft.AspNetCore.Mvc;
using ShopFront.Services;

namespace ShopFront.Controllers;

// Админ-панель: сводка по складу и товарам.
public class AdminController : Controller
{
    private readonly IProductRepository _products;
    private readonly ICartService _cart;

    public AdminController(IProductRepository products, ICartService cart)
    {
        _products = products;
        _cart = cart;
    }

    // GET /Admin
    public IActionResult Index()
    {
        var all = _products.GetAll();

        ViewBag.Total = all.Count;
        ViewBag.OutOfStock = all.Count(p => !p.InStock);
        ViewBag.Hits = all.Count(p => p.IsHit);
        ViewBag.BigDiscounts = all.Count(p => p.DiscountPercent > 30);
        ViewBag.StockValue = all.Sum(p => p.Price * p.Stock);
        ViewBag.CartCount = _cart.Get().TotalCount;

        return View(all);
    }
}
