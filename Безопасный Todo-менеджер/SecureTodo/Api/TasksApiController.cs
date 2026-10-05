using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTodo.Data;

namespace SecureTodo.Api;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/tasks")]
public sealed class TasksApiController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ApiTaskDto>), 200), ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var query = db.Tasks.AsNoTracking().AsQueryable();
        if (!User.IsInRole("Admin")) query = query.Where(x => x.UserId == userId);
        var tasks = await query.OrderByDescending(x => x.CreatedAt).Select(x =>
            new ApiTaskDto(x.Id, x.Title, x.Description, x.CreatedAt, x.IsCompleted, x.UserId)).ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiTaskDto), 200), ProducesResponseType(401), ProducesResponseType(404)]
    public async Task<IActionResult> Get(int id)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (task is null || (!User.IsInRole("Admin") && task.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))) return NotFound();
        return Ok(ToDto(task));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiTaskDto), 201), ProducesResponseType(400), ProducesResponseType(401)]
    public async Task<IActionResult> Create(SaveTaskRequest input)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        var task = new TaskItem { Title = input.Title.Trim(), Description = input.Description,
            IsCompleted = input.IsCompleted, UserId = userId, CreatedAt = DateTime.UtcNow };
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return Created($"/api/tasks/{task.Id}", ToDto(task));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(401), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, SaveTaskRequest input)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!CanEdit(task)) return StatusCode(403);
        task.Title = input.Title.Trim(); task.Description = input.Description; task.IsCompleted = input.IsCompleted;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204), ProducesResponseType(401), ProducesResponseType(403), ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!CanEdit(task)) return StatusCode(403);
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private bool CanEdit(TaskItem task)
        => User.IsInRole("Admin") || task.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier);
    private static ApiTaskDto ToDto(TaskItem task)
        => new(task.Id, task.Title, task.Description, task.CreatedAt, task.IsCompleted, task.UserId);
}
