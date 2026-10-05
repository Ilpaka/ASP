using LiveMonitor.Hubs;
using LiveMonitor.Models;
using LiveMonitor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<ConnectionRegistry>();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("Frontend");

app.MapHub<MonitoringHub>("/hubs/monitor");

app.MapPost("/api/notifications/system", async (SystemNotificationRequest request, ConnectionRegistry registry) =>
{
    var text = ValidateText(request.Text);
    if (text is null) return Results.Problem("Укажите сообщение от 1 до 500 символов.", statusCode: 400);
    await registry.SendSystemMessageAsync(text);
    return Results.Accepted();
});

app.MapPost("/api/notifications/room", async (RoomNotificationRequest request, ConnectionRegistry registry) =>
{
    var text = ValidateText(request.Text);
    var room = ValidateRoom(request.Room);
    if (text is null || room is null) return Results.Problem("Укажите комнату (1–40 символов) и сообщение (1–500 символов).", statusCode: 400);
    await registry.SendRoomNotificationAsync(room, text);
    return Results.Accepted();
});

app.MapPost("/api/notifications/private", async (PrivateNotificationRequest request, ConnectionRegistry registry) =>
{
    var text = ValidateText(request.Text);
    if (text is null || string.IsNullOrWhiteSpace(request.ConnectionId))
        return Results.Problem("Укажите ID получателя и сообщение от 1 до 500 символов.", statusCode: 400);
    if (!registry.Contains(request.ConnectionId)) return Results.Problem("Получатель не подключён.", statusCode: 404);
    await registry.SendPrivateMessageAsync(request.ConnectionId, "HTTP", text);
    return Results.Accepted();
});

app.Run();

static string? ValidateText(string? text)
{
    text = text?.Trim();
    return text is { Length: >= 1 and <= 500 } ? text : null;
}

static string? ValidateRoom(string? room)
{
    room = room?.Trim();
    return room is { Length: >= 1 and <= 40 } && !room.Any(char.IsControl) ? room : null;
}

public partial class Program { }
