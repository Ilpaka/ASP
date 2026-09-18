# Практическая работа №5 — CatalogAjax

AJAX-интерактивность каталога на **ASP.NET Core MVC (.NET 8)** + vanilla JS (без jQuery).
Построено на витрине из практической №4. Отчёт — в [`ОТЧЁТ.md`](ОТЧЁТ.md).

## Возможности

- **Живой поиск** с дебаунсом 300 мс, спиннером и обработкой ошибок.
- **Добавление в корзину** через AJAX: без перезагрузки, с toast-уведомлением.
- **Счётчик корзины** синхронизируется с сервером при загрузке любой страницы.
- **Бесконечная подгрузка** через `IntersectionObserver` (по 6 товаров).
- **Модалка товара** с параллельной загрузкой деталей и отзывов через `Promise.all`.
- Корзина в **сессии** — у каждого посетителя своя.

## Запуск

```bash
cd CatalogAjax
dotnet run --urls "http://localhost:5080"
```

Без установленного .NET — через Docker, из папки `Практическая-работа-5`:

```bash
docker run --rm -it -p 5080:5000 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -v "$PWD/CatalogAjax":/app -w /app \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet run --no-launch-profile --urls "http://0.0.0.0:5000"
```

Открыть <http://localhost:5080/>.

## Проверка эндпоинтов из терминала

```bash
# поиск — вернётся HTML-фрагмент без layout
curl "http://localhost:5080/Catalog/Search?query=алгорит"

# счётчик корзины
curl -c j -b j http://localhost:5080/Catalog/GetCartCount

# добавление в корзину
curl -c j -b j -X POST -d "id=1" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  http://localhost:5080/Catalog/AddToCart

# следующая порция товаров (пусто = конец)
curl "http://localhost:5080/Catalog/LoadMore?page=2"

# данные для модалки
curl "http://localhost:5080/Catalog/GetProductDetails?id=1"
curl "http://localhost:5080/Catalog/GetProductReviews?id=1"
```

## Структура

```
CatalogAjax/
├── Controllers/CatalogController.cs   # Search, AddToCart, GetCartCount,
│                                      # LoadMore, GetProductDetails, GetProductReviews
├── Services/
│   ├── InMemoryProductRepository.cs   # 24 товара + отзывы, пагинация, поиск
│   └── SessionCart.cs                 # корзина в сессии
├── Views/
│   ├── Catalog/Index.cshtml           # searchInput, catalogGrid, sentinel, модалка
│   └── Shared/
│       ├── _Layout.cshtml             # cartBadge, toast, site.js
│       ├── _ProductCard.cshtml        # data-product-id, add-to-cart
│       └── _ProductList.cshtml        # partial для AJAX-ответов
├── wwwroot/js/
│   ├── site.js                        # бейдж корзины (глобально)
│   ├── search.js                      # живой поиск
│   ├── cart.js                        # добавление в корзину + toast
│   ├── infinite.js                    # бесконечная подгрузка
│   └── product-modal.js               # модалка через Promise.all
└── bundleconfig.json                  # site.min.css, site.min.js, catalog.min.js
```
