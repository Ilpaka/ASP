using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.DTOs;

public sealed record CommentDto(int Id, int TaskItemId, string AuthorUsername, string Content, DateTime CreatedAt);

public sealed class CreateCommentDto
{
    [Required, StringLength(500, MinimumLength = 1)] public string Content { get; set; } = "";
    [Range(1, int.MaxValue)] public int AuthorId { get; set; }
}
