using System.Diagnostics;
using TaskBoard.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Шаг 3.2. Регистрация сервиса в DI.
// Singleton — один экземпляр на всё приложение, поэтому список задач общий для всех
// запросов и живёт, пока работает приложение (при перезапуске данные теряются — они
// хранятся в памяти). Scoped создавал бы новый список на каждый запрос, и задачи бы
// не сохранялись между запросами.
builder.Services.AddSingleton<ITaskService, InMemoryTaskService>();

var app = builder.Build();

// ---- Шаг 2. Замер времени обработки ----
// Стоит ДО логирующего middleware (ближе к началу конвейера), чтобы измерить полное
// время прохождения запроса через все последующие middleware. Если поставить ПОСЛЕ
// логирующего — замер не охватит его работу и часть конвейера.
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    // Заголовок нужно записать до отправки тела ответа (иначе заголовки уже
    // зафиксированы). OnStarting срабатывает прямо перед отправкой ответа.
    context.Response.OnStarting(() =>
    {
        sw.Stop();
        context.Response.Headers["X-Response-Time-ms"] = sw.ElapsedMilliseconds.ToString();
        return Task.CompletedTask;
    });
    await next(context);
});

// ---- Шаг 1 (усложнение). Короткое замыкание для /health ----
// Возвращает 200 OK и НЕ передаёт запрос дальше (нет вызова next).
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("healthy");
        return; // запрос не идёт дальше по конвейеру
    }
    await next(context);
});

// ---- Шаг 1. Первый middleware: заголовок + логирование ----
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-App-Name", "TaskBoard");

    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("--> {Method} {Path}", context.Request.Method, context.Request.Path);

    await next(context);

    logger.LogInformation("<-- {StatusCode}", context.Response.StatusCode);
});

// ---- Уровень 2 (среднее). Защита JSON API ключом X-Api-Key ----
// Для любых запросов к /tasks/api/* требуется заголовок X-Api-Key: secret123,
// иначе 401 Unauthorized.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/tasks/api"))
    {
        var key = context.Request.Headers["X-Api-Key"].ToString();
        if (key != "secret123")
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: неверный или отсутствующий X-Api-Key");
            return;
        }
    }
    await next(context);
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
