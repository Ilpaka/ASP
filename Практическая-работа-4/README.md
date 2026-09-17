# Практическая работа №4 — ShopFront (витрина товаров с админ-панелью)

**Темы:** CSS и Bootstrap в ASP.NET Core; bundling и minification; static files middleware;
wwwroot; Bootstrap grid и компоненты (navbar, cards, modal, alert).
Проект: `ShopFront` (ASP.NET Core MVC, .NET 8).

## Что реализовано

| Задание | Содержание | Где |
|---|---|---|
| 1 | Структура wwwroot (css/js/img), `UseStaticFiles()`, environment-секции, `bundleconfig.json` + BuildBundlerMinifier | `Program.cs`, `_Layout.cshtml`, `bundleconfig.json` |
| 2 | Navbar: тёмная тема, логотип, меню, кнопка «Войти», бургер | `Views/Shared/_Layout.cshtml` |
| 3 | Сетка `container/row/col-12 col-md-6 col-lg-4` и карточки товаров | `Views/Catalog/Index.cshtml`, `_ProductCard.cshtml` |
| 4 | Alert: «Корзина пуста» (warning) и «Товар добавлен» (success) + закрытие | `_AlertPartial.cshtml`, `Views/Cart/Index.cshtml` |
| 5 | Modal «Подтверждение заказа» со сводкой и кнопками | `Views/Cart/Index.cshtml` |
| 6 | Кастомные стили, дополняющие Bootstrap | `wwwroot/css/site.css` |

**Дополнительные задания (на «отлично»):**
- **Лёгкий** — адаптивная таблица цен в карточке (цена / без скидки / выгода) через `list-group`,
  вертикально на мобильных и в строку с `md`.
- **Средний** — тег-фильтр «Электроника / Одежда / Книги» на pill-кнопках; фильтрация на сервере,
  мгновенная подсветка активного тега в `site.js`.
- **Сложный** — динамические состояния карточки через Razor: `Stock == 0` → `border-danger`,
  кнопка `disabled`, бейдж «Нет в наличии»; `IsHit` → бейдж «ХИТ» + `shadow`;
  скидка > 30% → зелёная рамка + бейдж «Большая скидка».

Сверх методички: админ-панель `/Admin` со сводкой по складу и таблицей товаров
(в методичке ссылка «Админ» была заглушкой).

## Запуск

```bash
cd ShopFront
dotnet run --urls "http://localhost:5080"
```

Открыть <http://localhost:5080/>.

Без установленного .NET — через Docker, из папки `Практическая-работа-4`:

```bash
docker run --rm -it -p 5080:5000 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -v "$PWD/ShopFront":/app -w /app \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet run --no-launch-profile --urls "http://0.0.0.0:5000"
```

Страницы: `/` — каталог, `/Cart` — корзина, `/Admin` — админ-панель.

## Про бандлинг

`bundleconfig.json` описывает сборку `site.css → site.min.css` и `site.js → site.min.js`.
Пакет `BuildBundlerMinifier` подключён в `.csproj` и запускает минификацию **на этапе сборки**
(в логе `dotnet build` видно `Minified wwwroot/css/site.min.css`).

В `_Layout.cshtml` файлы подключаются по окружению:
в **Development** — исходные `bootstrap.css` / `site.css`,
в остальных — `bootstrap.min.css` / `site.min.css` с `asp-append-version="true"` (версионирование для кэша).

## Структура

```
ShopFront/
├── bundleconfig.json
├── Program.cs
├── Controllers/{Catalog,Cart,Admin}Controller.cs
├── Models/Product.cs            # Product, CartItem, CartViewModel, AlertMessage
├── Services/                    # IProductRepository, ICartService + реализации
├── Views/
│   ├── Catalog/Index.cshtml
│   ├── Cart/{Index,ThankYou}.cshtml
│   ├── Admin/Index.cshtml
│   └── Shared/{_Layout,_ProductCard,_AlertPartial}.cshtml
└── wwwroot/
    ├── css/{site.css, site.min.css}
    ├── js/{site.js, site.min.js}
    └── img/*.svg                # локальные SVG-заглушки
```
