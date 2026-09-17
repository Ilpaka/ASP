using Microsoft.AspNetCore.Mvc;
using ShopFront.Services;

namespace ShopFront.Controllers;

public class CartController : Controller
{
    private readonly IProductRepository _products;
    private readonly ICartService _cart;

    public CartController(IProductRepository products, ICartService cart)
    {
        _products = products;
        _cart = cart;
    }

    // GET /Cart
    public IActionResult Index()
    {
        ViewBag.CartCount = _cart.Get().TotalCount;
        return View(_cart.Get());
    }

    // POST /Cart/Add/5 — добавление товара из каталога
    [HttpPost]
    public IActionResult Add(int id, string? returnTag)
    {
        var product = _products.GetById(id);
        if (product == null) return NotFound();
        if (!product.InStock) return BadRequest("Товара нет в наличии");

        _cart.Add(product);

        // Флаг для alert-success «Товар добавлен в корзину»
        TempData["Added"] = product.Name;

        return RedirectToAction("Index", "Cart");
    }

    // POST /Cart/Remove/5
    [HttpPost]
    public IActionResult Remove(int id)
    {
        _cart.Remove(id);
        return RedirectToAction(nameof(Index));
    }

    // POST /Cart/Checkout — подтверждение заказа из модального окна
    [HttpPost]
    public IActionResult Checkout()
    {
        var cart = _cart.Get();
        if (!cart.Items.Any()) return RedirectToAction(nameof(Index));

        TempData["OrderTotal"] = cart.TotalAmount.ToString("C");
        TempData["OrderCount"] = cart.TotalCount;
        _cart.Clear();

        return RedirectToAction(nameof(ThankYou));
    }

    // GET /Cart/ThankYou
    public IActionResult ThankYou()
    {
        ViewBag.CartCount = _cart.Get().TotalCount;
        return View();
    }
}
