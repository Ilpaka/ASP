using Microsoft.AspNetCore.Mvc;
using ShopFront.Services;

namespace ShopFront.Controllers;

public class CatalogController : Controller
{
    private readonly IProductRepository _products;
    private readonly ICartService _cart;

    public CatalogController(IProductRepository products, ICartService cart)
    {
        _products = products;
        _cart = cart;
    }

    // GET /  и  /Catalog?tag=Электроника  — фильтрация по тегу (доп. уровень «Средний»)
    public IActionResult Index(string tag = "all")
    {
        var all = _products.GetAll();

        var items = tag == "all"
            ? all
            : all.Where(p => p.Category == tag).ToList();

        ViewBag.Tag = tag;
        ViewBag.Categories = _products.GetCategories();
        ViewBag.CartCount = _cart.Get().TotalCount;

        return View(items);
    }
}
