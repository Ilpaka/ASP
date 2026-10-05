namespace SecureTodo.Data;

public sealed class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = "";
    public string UserId { get; set; } = "";
    public AppUser User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
