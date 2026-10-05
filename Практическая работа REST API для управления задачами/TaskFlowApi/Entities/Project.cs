namespace TaskFlowApi.Entities;

public sealed class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TaskItem> Tasks { get; set; } = [];
}
