using CatalogAjax.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAjax.Controllers;

public class CartController : Controller
{
    private readonly IProductRepository _repo;

    public CartController(IProductRepository repo)
    {
        _repo = repo;
    }

    // GET /Cart
    public IActionResult Index()
    {
        ViewBag.CartCount = SessionCart.Count(HttpContext.Session);
        return View(SessionCart.Build(HttpContext.Session, _repo));
    }

    // POST /Cart/Remove/5 — убрать одну единицу
    [HttpPost]
    public IActionResult Remove(int id)
    {
        SessionCart.RemoveOne(HttpContext.Session, id);
        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/Checkout — подтверждение из модального окна
    [HttpPost]
    public IActionResult Checkout()
    {
        var cart = SessionCart.Build(HttpContext.Session, _repo);
        if (!cart.Items.Any()) return RedirectToAction(nameof(Index));

        TempData["OrderTotal"] = cart.TotalAmount.ToString("C0");
        TempData["OrderCount"] = cart.TotalCount;
        SessionCart.Clear(HttpContext.Session);

        return RedirectToAction(nameof(ThankYou));
    }

    // GET /Cart/ThankYou
    public IActionResult ThankYou()
    {
        ViewBag.CartCount = SessionCart.Count(HttpContext.Session);
        return View();
    }
}
