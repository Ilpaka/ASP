# Практическая работа №3 — DashboardAdmin

Админ-панель с динамическими дашбордами на **ASP.NET Core MVC (.NET 8)**.
Фокус — Razor-синтаксис, многоуровневые Layouts и ViewComponents.
Отчёт — в [`ОТЧЁТ.md`](ОТЧЁТ.md).

## Возможности

- Карточки показателей (6 шт., один отрицательный) из Singleton-репозитория.
- Многоуровневый layout: `_Layout` → `_AppLayout` (двухколоночный с боковой панелью).
- ViewComponent «Сводка по трендам» в сайдбаре (группировка по тренду).
- Три способа рендеринга карточки: напрямую в цикле, через partial, через шаблонный делегат.
- Секции `PageHeader`, `Actions`, `Scripts` с условным рендерингом и пробросом.
- Режим печати `/Dashboard/Print` через динамический выбор layout в `_ViewStart`.

## Запуск (если установлен .NET SDK 8)

```bash
cd DashboardAdmin
dotnet run
```

Открыть <http://localhost:5000/> (дашборд) и <http://localhost:5000/Dashboard/Print> (печать).

## Запуск без установленного .NET (через Docker)

Из папки `Практическая-работа-3`:

```bash
docker run --rm -it -p 5000:5000 \
  -e ASPNETCORE_URLS=http://+:5000 -e ASPNETCORE_ENVIRONMENT=Development \
  -v "$PWD/DashboardAdmin":/app -w /app \
  mcr.microsoft.com/dotnet/sdk:8.0 dotnet run
```

## Структура

```
DashboardAdmin/
├── Program.cs
├── Controllers/DashboardController.cs      # Index + Print
├── Models/
│   ├── DashboardCard.cs                     # DashboardCard + enum Trend
│   └── DashboardViewModel.cs                # TrendSummaryItem
├── Services/
│   ├── IDashboardRepository.cs
│   └── InMemoryDashboardRepository.cs       # 6 карточек, Singleton
├── ViewComponents/TrendSummaryViewComponent.cs
└── Views/
    ├── _ViewStart.cshtml                     # динамический выбор layout
    ├── _ViewImports.cshtml
    ├── Dashboard/Index.cshtml                # Razor-логика, делегаты, секции
    └── Shared/
        ├── _Layout.cshtml                    # базовый
        ├── _AppLayout.cshtml                 # двухколоночный (сайдбар)
        ├── _PrintLayout.cshtml               # режим печати
        ├── _DashboardCard.cshtml             # partial карточки
        └── Components/TrendSummary/Default.cshtml
```
