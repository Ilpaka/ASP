namespace TaskFlowApi.Entities;

public sealed class Comment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public int AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
