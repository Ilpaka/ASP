namespace TaskFlowApi.Entities;

public sealed class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public List<TaskItem> AssignedTasks { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
}
