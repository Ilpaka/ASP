using System.Runtime.CompilerServices;
using LiveMonitor.Services;
using Microsoft.AspNetCore.SignalR;

namespace LiveMonitor.Hubs;

public sealed class MonitoringHub(ConnectionRegistry registry) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await registry.RegisterAsync(Context.ConnectionId);
        await Clients.Others.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await registry.UnregisterAsync(Context.ConnectionId);
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public Task SendMessage(string text)
    {
        text = ValidText(text);
        return Clients.All.SendAsync("MessageReceived", Context.ConnectionId, text);
    }

    public async Task JoinRoom(string room)
    {
        room = ValidRoom(room);
        var previous = registry.GetRoom(Context.ConnectionId);
        if (previous is null) throw new HubException("Подключение не найдено.");
        if (!string.IsNullOrEmpty(previous) && previous != room)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, previous);
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        await registry.UpdateRoomAsync(Context.ConnectionId, room);
        await Clients.Caller.SendAsync("JoinedRoom", room);
    }

    public async Task LeaveRoom(string room)
    {
        room = ValidRoom(room);
        if (registry.GetRoom(Context.ConnectionId) != room)
            throw new HubException("Вы не состоите в этой комнате.");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
        await registry.UpdateRoomAsync(Context.ConnectionId, "");
        await Clients.Caller.SendAsync("LeftRoom", room);
    }

    public Task SendRoomMessage(string room, string text)
    {
        room = ValidRoom(room);
        text = ValidText(text);
        if (registry.GetRoom(Context.ConnectionId) != room)
            throw new HubException("Сначала войдите в комнату.");
        return Clients.Group(room).SendAsync("RoomMessageReceived", Context.ConnectionId, room, text);
    }

    public async Task SendPrivateMessage(string targetConnectionId, string text)
    {
        text = ValidText(text);
        if (string.IsNullOrWhiteSpace(targetConnectionId) || !registry.Contains(targetConnectionId))
            throw new HubException("Получатель сейчас не подключён.");
        await registry.SendPrivateMessageAsync(targetConnectionId, Context.ConnectionId, text);
        await Clients.Caller.SendAsync("PrivateMessageSent", targetConnectionId, text);
    }

    public Task Typing(string room)
    {
        room = ValidRoom(room);
        if (registry.GetRoom(Context.ConnectionId) != room)
            throw new HubException("Сначала войдите в комнату.");
        return Clients.OthersInGroup(room).SendAsync("UserTyping", Context.ConnectionId, room);
    }

    public Task TypingGlobal()
        => Clients.Others.SendAsync("UserTyping", Context.ConnectionId, "");

    public async IAsyncEnumerable<int> StreamNumbers(int max,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (max is < 1 or > 1000) throw new HubException("Максимум должен быть от 1 до 1000.");
        while (!cancellationToken.IsCancellationRequested)
        {
            try { await Task.Delay(500, cancellationToken); }
            catch (OperationCanceledException) { yield break; }
            yield return Random.Shared.Next(1, max + 1);
        }
    }

    private static string ValidText(string? text)
    {
        text = text?.Trim() ?? "";
        if (text.Length is < 1 or > 500) throw new HubException("Сообщение должно содержать от 1 до 500 символов.");
        return text;
    }

    private static string ValidRoom(string? room)
    {
        room = room?.Trim() ?? "";
        if (room.Length is < 1 or > 40 || room.Any(char.IsControl))
            throw new HubException("Название комнаты должно содержать от 1 до 40 символов без управляющих символов.");
        return room;
    }
}
