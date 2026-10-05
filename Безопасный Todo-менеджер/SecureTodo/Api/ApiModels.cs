using System.ComponentModel.DataAnnotations;

namespace SecureTodo.Api;

public sealed class ApiLoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string? TwoFactorCode { get; set; }
}

public sealed record RefreshRequest(string RefreshToken);
public sealed record ApiTaskDto(int Id, string Title, string? Description, DateTime CreatedAt, bool IsCompleted, string UserId);

public sealed class SaveTaskRequest
{
    [Required, StringLength(120, MinimumLength = 3)] public string Title { get; set; } = "";
    [StringLength(2000)] public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
