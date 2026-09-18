using CatalogAjax.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAjax.Controllers;

public class CatalogController : Controller
{
    private readonly IProductRepository _repo;

    // Товаров на одну «порцию» при бесконечной подгрузке
    private const int PageSize = 6;

    public CatalogController(IProductRepository repo)
    {
        _repo = repo;
    }

    // GET /  — первая страница каталога (остальное догружается через AJAX)
    public IActionResult Index(string tag = "all")
    {
        var products = _repo.GetPage(1, PageSize, tag);

        ViewBag.Tag = tag;
        ViewBag.Categories = _repo.GetCategories();
        ViewBag.PageSize = PageSize;
        ViewBag.CartCount = SessionCart.Count(HttpContext.Session);

        return View(products);
    }

    // ---------- Задание 1. AJAX-эндпоинты ----------

    // 1.1 GET /Catalog/Search?query=ноут — отдаёт HTML-фрагмент (partial без layout)
    [HttpGet]
    public IActionResult Search(string query)
    {
        var products = _repo.Search(query);
        return PartialView("_ProductList", products);
    }

    // 1.2 POST /Catalog/AddToCart — отдаёт JSON
    [HttpPost]
    public IActionResult AddToCart(int id)
    {
        var product = _repo.GetById(id);
        if (product == null)
            return Json(new { success = false, message = "Товар не найден" });

        if (!product.InStock)
            return Json(new { success = false, message = "Товара нет в наличии" });

        SessionCart.Add(HttpContext.Session, id);

        return Json(new
        {
            success = true,
            cartCount = SessionCart.Count(HttpContext.Session),
            productName = product.Name
        });
    }

    // 1.3 GET /Catalog/GetCartCount — отдаёт JSON с количеством товаров в корзине
    [HttpGet]
    public IActionResult GetCartCount()
    {
        return Json(new { count = SessionCart.Count(HttpContext.Session) });
    }

    // ---------- Средний уровень. Бесконечная подгрузка ----------

    // GET /Catalog/LoadMore?page=2 — следующая порция карточек (пустой ответ = данных больше нет)
    [HttpGet]
    public IActionResult LoadMore(int page, string tag = "all")
    {
        var products = _repo.GetPage(page, PageSize, tag);

        // Пустой результат -> пустая строка; JS по ней понимает, что грузить больше нечего
        if (products.Count == 0)
            return Content(string.Empty, "text/html");

        return PartialView("_ProductList", products);
    }

    // ---------- Сложный уровень. Данные для модального окна ----------

    // GET /Catalog/GetProductDetails?id=1 — JSON с описанием товара
    [HttpGet]
    public IActionResult GetProductDetails(int id)
    {
        var p = _repo.GetById(id);
        if (p == null) return NotFound(new { message = "Товар не найден" });

        return Json(new
        {
            id = p.Id,
            name = p.Name,
            price = p.Price.ToString("C0"),
            description = p.Description,
            imageUrl = p.ImageUrl,
            category = p.Category,
            inStock = p.InStock,
            stock = p.Stock,
            discountPercent = p.DiscountPercent
        });
    }

    // GET /Catalog/GetProductReviews?id=1 — JSON со списком отзывов
    [HttpGet]
    public IActionResult GetProductReviews(int id)
    {
        var reviews = _repo.GetReviews(id)
            .Select(r => new { author = r.Author, text = r.Text, rating = r.Rating })
            .ToList();

        return Json(reviews);
    }
}
