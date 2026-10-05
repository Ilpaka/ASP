using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.DTOs;
using TaskFlowApi.Entities;
using TaskFlowApi.Mapping;

namespace TaskFlowApi.Controllers;

/// <summary>Комментарии к задачам.</summary>
[ApiController, ApiVersion(1.0, Deprecated = true), ApiVersion(2.0)]
public sealed class CommentsController(AppDbContext db) : ControllerBase
{
    /// <summary>Возвращает комментарии задачи.</summary>
    [HttpGet("api/v{version:apiVersion}/tasks/{taskId:int}/comments")]
    [HttpGet("api/tasks/{taskId:int}/comments")]
    [ProducesResponseType(typeof(List<CommentDto>), 200), ProducesResponseType(404)]
    public async Task<ActionResult<List<CommentDto>>> GetAll(int taskId, CancellationToken ct)
    {
        if (!await db.Tasks.AnyAsync(x => x.Id == taskId, ct)) return Problem(statusCode: 404, title: "Resource not found", detail: "Задача не найдена.");
        var comments = await db.Comments.AsNoTracking().Include(x => x.Author).Where(x => x.TaskItemId == taskId).OrderBy(x => x.Id).ToListAsync(ct);
        return Ok(comments.Select(x => x.ToDto()).ToList());
    }

    /// <summary>Добавляет комментарий к задаче.</summary>
    [HttpPost("api/v{version:apiVersion}/tasks/{taskId:int}/comments")]
    [HttpPost("api/tasks/{taskId:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), 201), ProducesResponseType(400), ProducesResponseType(404)]
    public async Task<ActionResult<CommentDto>> Create(int taskId, CreateCommentDto input, CancellationToken ct)
    {
        if (!await db.Tasks.AnyAsync(x => x.Id == taskId, ct)) return Problem(statusCode: 404, title: "Resource not found", detail: "Задача не найдена.");
        var author = await db.Users.FindAsync([input.AuthorId], ct);
        if (author is null) return ValidationProblem("AuthorId указывает на несуществующего пользователя.");
        var comment = new Comment { TaskItemId = taskId, AuthorId = input.AuthorId, Author = author, Content = input.Content.Trim(), CreatedAt = DateTime.UtcNow };
        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);
        var version = RouteData.Values["version"]?.ToString();
        var location = version is null ? $"/api/tasks/{taskId}/comments" : $"/api/v{version}/tasks/{taskId}/comments";
        return Created(location, comment.ToDto());
    }

    /// <summary>Удаляет комментарий.</summary>
    [HttpDelete("api/v{version:apiVersion}/comments/{id:int}")]
    [HttpDelete("api/comments/{id:int}")]
    [ProducesResponseType(204), ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var comment = await db.Comments.FindAsync([id], ct);
        if (comment is null) return NotFound();
        db.Comments.Remove(comment);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
