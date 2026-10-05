using System.ComponentModel.DataAnnotations;

namespace SecureTodo.Data;

public sealed class TaskItem
{
    public int Id { get; set; }
    [Required, StringLength(120, MinimumLength = 3)] public string Title { get; set; } = "";
    [StringLength(2000)] public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; }
    [Required] public string UserId { get; set; } = "";
    public AppUser User { get; set; } = null!;
}
