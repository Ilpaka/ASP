namespace LiveMonitor.Models;

public sealed record SystemNotificationRequest(string? Text);
public sealed record RoomNotificationRequest(string? Room, string? Text);
public sealed record PrivateNotificationRequest(string? ConnectionId, string? Text);
public sealed record ConnectedUser(string ConnectionId, string Room);
