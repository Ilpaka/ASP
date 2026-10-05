namespace TaskFlowApi.Entities;

public enum TaskStatus { ToDo, InProgress, Done }
public enum Priority { Low, Medium, High }

public sealed class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public Priority Priority { get; set; } = Priority.Medium;
    public int? AssignedToId { get; set; }
    public AppUser? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Comment> Comments { get; set; } = [];
}
