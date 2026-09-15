# Практическая работа №2 — TaskBoard

Приложение-задачник на **ASP.NET Core MVC (.NET 8)** с middleware, DI, контроллером,
маршрутизацией, model binding и JSON API. Отчёт — в [`ОТЧЁТ.md`](ОТЧЁТ.md).

## Возможности

- Middleware: заголовок `X-App-Name`, логирование запросов, замер `X-Response-Time-ms`,
  short-circuit `/health`, защита API ключом `X-Api-Key` (Уровень 2).
- DI: `ITaskService` → `InMemoryTaskService` (Singleton).
- Страницы `/tasks` (список + счётчик + фильтр), `/tasks/create`, `/tasks/details/{id}`.
- JSON API: `GET /tasks/api/list`, `GET /tasks/api/{id}`, `POST /tasks/api/create`.
- Доп. уровни: счётчик (1), защита API ключом (2), фильтр `?status=` (3).

## Запуск (штатно, если установлен .NET SDK 8)

```bash
cd TaskBoard
dotnet run
```

Открыть <http://localhost:5000/tasks>.

## Запуск без установленного .NET (через Docker)

Из папки `Практическая-работа-2`:

```bash
docker run --rm -it -p 5000:5000 \
  -e ASPNETCORE_URLS=http://+:5000 -e ASPNETCORE_ENVIRONMENT=Development \
  -v "$PWD/TaskBoard":/app -w /app \
  mcr.microsoft.com/dotnet/sdk:8.0 dotnet run
```

Открыть <http://localhost:5000/tasks>.

## Проверка API через терминал

```bash
# без ключа -> 401
curl http://localhost:5000/tasks/api/list

# список (с ключом)
curl -H "X-Api-Key: secret123" http://localhost:5000/tasks/api/list

# создание
curl -X POST http://localhost:5000/tasks/api/create \
  -H "X-Api-Key: secret123" -H "Content-Type: application/json" \
  -d '{"title":"Тест через curl","description":"Создано из терминала"}'

# по ID
curl -H "X-Api-Key: secret123" http://localhost:5000/tasks/api/1

# health-check (short-circuit)
curl http://localhost:5000/health   # -> healthy
```

## Структура

```
Практическая-работа-2/
├── ОТЧЁТ.md
├── README.md
└── TaskBoard/
    ├── Program.cs                     # middleware + DI + маршрутизация
    ├── Controllers/TasksController.cs # действия + JSON API
    ├── Services/
    │   ├── ITaskService.cs            # TaskItem, ITaskService, CreateTaskRequest
    │   └── InMemoryTaskService.cs     # хранение в памяти
    ├── Views/Tasks/                   # Index, Details, Create (Razor)
    ├── Views/Shared/_Layout.cshtml    # пункт меню «Задачи»
    └── wwwroot/                       # Bootstrap и статика
```
