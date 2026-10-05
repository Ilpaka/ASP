namespace TaskFlowApi.Entities;

public sealed class IdempotencyRecord
{
    public string Key { get; set; } = "";
    public string RequestBodyHash { get; set; } = "";
    public string ResponseBody { get; set; } = "";
    public int StatusCode { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
}
