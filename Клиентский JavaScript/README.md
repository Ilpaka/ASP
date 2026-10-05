# Клиентский JavaScript — AJAX-панель

Самостоятельная практическая работа по презентации «Клиентский JavaScript: основы для AJAX». В презентации после теоретических слайдов последним слайдом указано только «Практическая работа», без отдельного детального условия. Поэтому проект демонстрирует перечисленные в презентации навыки в одном небольшом приложении.

## Реализовано

- ASP.NET Core MVC с первоначальной Razor-разметкой и статическими ресурсами.
- Чтение элементов и изменение DOM на клиенте без jQuery.
- Параллельные GET-запросы Fetch API к двум ресурсам дашборда через `Promise.all`.
- POST формы в JSON через `fetch`, `JSON.stringify`, заголовок `Content-Type` и `preventDefault()`.
- Проверка `response.ok`, разбор `response.json()`, `async/await` и обработка ошибок через `try/catch`.
- Обновление DOM с созданием узлов; текстовые поля экранируются через `textContent` перед HTML-выводом.
- Серверная проверка входных данных и понятные состояния загрузки, пустого списка и ошибки.
- Адаптивная аналитическая панель: KPI, график, последние события и форма добавления события.

## Запуск

Нужен .NET 8 SDK. В каталоге `ClientJavaScript` выполните:

```bash
dotnet run
```

Откройте адрес, напечатанный приложением. Данные хранятся в памяти процесса и сбрасываются после перезапуска.

## API

- `GET /api/dashboard/metrics` — показатели JSON.
- `GET /api/dashboard/activity` — последние события JSON.
- `POST /api/dashboard/activity` — добавить событие; тело `{ "title": "...", "category": "..." }`.

## Файлы

- `ClientJavaScript/Views/Home/Index.cshtml` — серверная оболочка страницы.
- `ClientJavaScript/wwwroot/js/dashboard.js` — события, Fetch API, Promise, DOM и форма.
- `ClientJavaScript/wwwroot/js/site.js` — общие DOM-помощники.
- `ClientJavaScript/Controllers/DashboardController.cs` — JSON API.
- `ClientJavaScript/Services/DashboardService.cs` — демонстрационные данные и добавление событий.
