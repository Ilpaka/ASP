using CatalogAjax.Models;

namespace CatalogAjax.Services;

public interface IProductRepository
{
    List<Product> GetAll();
    Product? GetById(int id);
    List<string> GetCategories();

    // Постраничная выдача — для бесконечной подгрузки
    List<Product> GetPage(int page, int pageSize, string tag = "all");

    // Поиск по названию и описанию — для live-поиска
    List<Product> Search(string? query);

    // Отзывы к товару — для модального окна
    List<Review> GetReviews(int productId);
}
