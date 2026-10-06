using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.DTOs;
using TaskFlowApi.Entities;
using TaskFlowApi.Exceptions;
using TaskFlowApi.Mapping;
using TaskStatus = TaskFlowApi.Entities.TaskStatus;

namespace TaskFlowApi.Controllers;

/// <summary>Задачи по контракту v2.</summary>
[ApiController, ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/tasks")]
public sealed class TasksV2Controller(AppDbContext db) : ControllerBase
{
    /// <summary>Возвращает страницу задач с фильтрацией и сортировкой.</summary>
    /// <param name="query">Страница, размер, статус, приоритет, сортировка и поиск.</param>
    /// <param name="ct">Токен отмены.</param>
    [HttpGet, ProducesResponseType(typeof(PagedResult<TaskItemV2Dto>), 200), ProducesResponseType(400)]
    public async Task<ActionResult<PagedResult<TaskItemV2Dto>>> GetAll([FromQuery] TaskQuery query, CancellationToken ct)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 50)
            return Problem(statusCode: 400, title: "Invalid pagination", detail: "page должен быть >= 1, pageSize — от 1 до 50.");
        var sortBy = query.SortBy?.ToLowerInvariant() ?? "createdat";
        var sortDir = query.SortDir?.ToLowerInvariant() ?? "desc";
        if (sortBy is not ("title" or "duedate" or "createdat") || sortDir is not ("asc" or "desc"))
            return Problem(statusCode: 400, title: "Invalid sorting", detail: "sortBy: title, dueDate, createdAt; sortDir: asc, desc.");

        IQueryable<TaskItem> items = db.Tasks.AsNoTracking();
        if (query.Status is not null) items = items.Where(x => x.Status == query.Status);
        if (query.Priority is not null) items = items.Where(x => x.Priority == query.Priority);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            items = items.Where(x => EF.Functions.Like(x.Title, $"%{search}%"));
        }
        var totalCount = await items.CountAsync(ct);
        items = (sortBy, sortDir) switch
        {
            ("title", "asc") => items.OrderBy(x => x.Title).ThenBy(x => x.Id),
            ("title", _) => items.OrderByDescending(x => x.Title).ThenBy(x => x.Id),
            ("duedate", "asc") => items.OrderBy(x => x.DueDate).ThenBy(x => x.Id),
            ("duedate", _) => items.OrderByDescending(x => x.DueDate).ThenBy(x => x.Id),
            ("createdat", "asc") => items.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            _ => items.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
        };
        var pageItems = await items.Include(x => x.AssignedTo).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return Ok(new PagedResult<TaskItemV2Dto>(pageItems.Select(x => x.ToV2Dto()).ToList(), query.Page, query.PageSize, totalCount));
    }

    /// <summary>Возвращает задачу v2 по идентификатору.</summary>
    [HttpGet("{id:int}"), ProducesResponseType(typeof(TaskItemV2Dto), 200), ProducesResponseType(404)]
    public async Task<ActionResult<TaskItemV2Dto>> Get(int id, CancellationToken ct)
    {
        var task = await db.Tasks.AsNoTracking().Include(x => x.AssignedTo).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (task is null) throw new NotFoundException("Задача не найдена.");
        return Ok(task.ToV2Dto());
    }

    /// <summary>Создаёт задачу v2. Требует заголовок X-Idempotency-Key с GUID.</summary>
    [HttpPost, ProducesResponseType(typeof(TaskItemV2Dto), 201), ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(409)]
    public async Task<ActionResult<TaskItemV2Dto>> Create(CreateTaskV2Dto input,
        [FromHeader(Name = "X-Idempotency-Key"), Required] Guid idempotencyKey, CancellationToken ct)
    {
        if (!await db.Projects.AnyAsync(x => x.Id == input.ProjectId, ct)) throw new NotFoundException("Проект не найден.");
        if (input.AssignedToId is not null && !await db.Users.AnyAsync(x => x.Id == input.AssignedToId, ct))
            return ValidationProblem("AssignedToId указывает на несуществующего пользователя.");
        var task = new TaskItem { ProjectId = input.ProjectId, Title = input.Title.Trim(), Description = input.Description,
            Status = input.Status, Priority = input.Priority, AssignedToId = input.AssignedToId, DueDate = input.DueDate, CreatedAt = DateTime.UtcNow };
        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
        if (task.AssignedToId is not null) await db.Entry(task).Reference(x => x.AssignedTo).LoadAsync(ct);
        return Created($"/api/v2/tasks/{task.Id}", task.ToV2Dto());
    }

    /// <summary>Полностью обновляет задачу v2.</summary>
    [HttpPut("{id:int}"), ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(422)]
    public async Task<IActionResult> Update(int id, UpdateTaskV2Dto input, CancellationToken ct)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) throw new NotFoundException("Задача не найдена.");
        if (input.AssignedToId is not null && !await db.Users.AnyAsync(x => x.Id == input.AssignedToId, ct))
            return ValidationProblem("AssignedToId указывает на несуществующего пользователя.");
        if (input.Status == TaskStatus.Done && task.Status != TaskStatus.Done && !await db.Comments.AnyAsync(x => x.TaskItemId == id, ct))
            throw new BusinessRuleException("Нельзя завершить задачу без комментариев");
        task.Title = input.Title.Trim(); task.Description = input.Description; task.Status = input.Status!.Value;
        task.Priority = input.Priority!.Value; task.AssignedToId = input.AssignedToId; task.DueDate = input.DueDate;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Меняет статус задачи. Завершение требует хотя бы один комментарий.</summary>
    [HttpPatch("{id:int}/status"), ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(404), ProducesResponseType(422)]
    public async Task<IActionResult> ChangeStatus(int id, UpdateTaskStatusDto input, CancellationToken ct)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) throw new NotFoundException("Задача не найдена.");
        if (input.Status == TaskStatus.Done && task.Status != TaskStatus.Done && !await db.Comments.AnyAsync(x => x.TaskItemId == id, ct))
            throw new BusinessRuleException("Нельзя завершить задачу без комментариев");
        task.Status = input.Status!.Value;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Удаляет задачу v2 и её комментарии.</summary>
    [HttpDelete("{id:int}"), ProducesResponseType(204), ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var task = await db.Tasks.FindAsync([id], ct);
        if (task is null) throw new NotFoundException("Задача не найдена.");
        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
