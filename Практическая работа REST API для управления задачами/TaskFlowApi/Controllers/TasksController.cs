using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.DTOs;
using TaskFlowApi.Entities;
using TaskFlowApi.Mapping;

namespace TaskFlowApi.Controllers;

/// <summary>Задачи по контракту v1.</summary>
[ApiController, ApiVersion(1.0, Deprecated = true)]
[Route("api/v{version:apiVersion}/tasks")]
[Route("api/tasks")]
public sealed class TasksController(AppDbContext db) : ControllerBase
{
    /// <summary>Возвращает все задачи.</summary>
    [HttpGet, ProducesResponseType(typeof(List<TaskItemDto>), 200)]
    public async Task<ActionResult<List<TaskItemDto>>> GetAll(CancellationToken ct)
        => Ok((await db.Tasks.AsNoTracking().Include(x => x.AssignedTo).OrderBy(x => x.Id).ToListAsync(ct)).Select(x => x.ToV1Dto()).ToList());

    /// <summary>Возвращает задачу по идентификатору.</summary>
    [HttpGet("{id:int}"), ProducesResponseType(typeof(TaskItemDto), 200), ProducesResponseType(404)]
    public async Task<ActionResult<TaskItemDto>> Get(int id, CancellationToken ct)
    {
        var task = await db.Tasks.AsNoTracking().Include(x => x.AssignedTo).FirstOrDefaultAsync(x => x.Id == id, ct);
        return task is null ? NotFound() : Ok(task.ToV1Dto());
    }

    /// <summary>Создаёт задачу со статусом ToDo по умолчанию.</summary>
    [HttpPost, ProducesResponseType(typeof(TaskItemDto), 201), ProducesResponseType(400), ProducesResponseType(404)]
    public async Task<ActionResult<TaskItemDto>> Create(CreateTaskDto input, CancellationToken ct)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == input.ProjectId, ct)) return NotFound();
        if (input.AssignedToId is not null && !await db.Users.AnyAsync(x => x.Id == input.AssignedToId, ct)) return ValidationProblem("AssignedToId указывает на несуществующего пользователя.");
        var task = new TaskItem { ProjectId = input.ProjectId, Title = input.Title.Trim(), Description = input.Description, Status = input.Status, AssignedToId = input.AssignedToId, DueDate = input.DueDate, CreatedAt = DateTime.UtcNow };
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
        if (task.AssignedToId is not null) await db.Entry(task).Reference(x => x.AssignedTo).LoadAsync(ct);
        var version = RouteData.Values["version"]?.ToString();
        var location = version is null ? $"/api/tasks/{task.Id}" : $"/api/v{version}/tasks/{task.Id}";
        return Created(location, task.ToV1Dto());
    }

    /// <summary>Полностью обновляет задачу. Статус обязателен.</summary>
    [HttpPut("{id:int}"), ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateTaskDto input, CancellationToken ct)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) return NotFound();
        if (input.AssignedToId is not null && !await db.Users.AnyAsync(x => x.Id == input.AssignedToId, ct)) return ValidationProblem("AssignedToId указывает на несуществующего пользователя.");
        task.Title = input.Title.Trim(); task.Description = input.Description; task.Status = input.Status!.Value;
        task.AssignedToId = input.AssignedToId; task.DueDate = input.DueDate;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Удаляет задачу вместе с комментариями.</summary>
    [HttpDelete("{id:int}"), ProducesResponseType(204), ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) return NotFound();
        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
