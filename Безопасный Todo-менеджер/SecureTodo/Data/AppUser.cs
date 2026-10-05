using Microsoft.AspNetCore.Identity;

namespace SecureTodo.Data;

public sealed class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public List<TaskItem> Tasks { get; set; } = [];
}
