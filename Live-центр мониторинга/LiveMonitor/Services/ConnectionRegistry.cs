using System.Collections.Concurrent;
using LiveMonitor.Hubs;
using LiveMonitor.Models;
using Microsoft.AspNetCore.SignalR;

namespace LiveMonitor.Services;

/// <summary>Хранит подключения и отправляет события из хаба и обычных HTTP-обработчиков.</summary>
public sealed class ConnectionRegistry(IHubContext<MonitoringHub> hubContext)
{
    private readonly ConcurrentDictionary<string, string> _connections = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _presenceGate = new(1, 1);

    public bool Contains(string connectionId) => _connections.ContainsKey(connectionId);
    public string? GetRoom(string connectionId) => _connections.TryGetValue(connectionId, out var room) ? room : null;

    public async Task RegisterAsync(string connectionId)
    {
        _connections[connectionId] = "";
        await BroadcastPresenceAsync();
    }

    public async Task UnregisterAsync(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
        await BroadcastPresenceAsync();
    }

    public async Task UpdateRoomAsync(string connectionId, string room)
    {
        while (_connections.TryGetValue(connectionId, out var previous))
        {
            if (_connections.TryUpdate(connectionId, room, previous))
            {
                await BroadcastPresenceAsync();
                return;
            }
        }
        throw new HubException("Подключение не найдено.");
    }

    private async Task BroadcastPresenceAsync()
    {
        await _presenceGate.WaitAsync();
        try
        {
            var users = _connections
                .Select(x => new ConnectedUser(x.Key, string.IsNullOrEmpty(x.Value) ? "без комнаты" : x.Value))
                .OrderBy(x => x.ConnectionId, StringComparer.Ordinal)
                .ToArray();
            await hubContext.Clients.All.SendAsync("OnlineCountUpdated", users.Length);
            await hubContext.Clients.All.SendAsync("UsersUpdated", users);
        }
        finally { _presenceGate.Release(); }
    }

    public Task SendSystemMessageAsync(string text)
        => hubContext.Clients.All.SendAsync("SystemMessage", text, DateTimeOffset.UtcNow);

    public Task SendRoomNotificationAsync(string room, string text)
        => hubContext.Clients.Group(room).SendAsync("RoomMessageReceived", "HTTP", room, text);

    public Task SendPrivateMessageAsync(string connectionId, string senderId, string text)
        => hubContext.Clients.Client(connectionId).SendAsync("PrivateMessageReceived", senderId, text);
}
