namespace ClientJavaScript.Models;

public sealed record Metric(int Id, string Name, decimal Value, string Unit, string Description);
public sealed record Activity(int Id, string Title, string Category, DateTime CreatedAt);
public sealed record DashboardSnapshot(IReadOnlyList<Metric> Metrics, IReadOnlyList<Activity> Activities);
public sealed record CreateActivityRequest(string Title, string Category);
