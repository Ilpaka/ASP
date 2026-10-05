using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.DTOs;
using TaskFlowApi.Entities;
using TaskFlowApi.Mapping;

namespace TaskFlowApi.Controllers;

/// <summary>Управление проектами.</summary>
[ApiController, ApiVersion(1.0, Deprecated = true), ApiVersion(2.0)]
[Route("api/v{version:apiVersion}/projects")]
[Route("api/projects")]
public sealed class ProjectsController(AppDbContext db) : ControllerBase
{
    /// <summary>Возвращает список проектов.</summary>
    [HttpGet, ProducesResponseType(typeof(List<ProjectDto>), 200)]
    public async Task<ActionResult<List<ProjectDto>>> GetAll(CancellationToken ct)
        => Ok((await db.Projects.AsNoTracking().Include(x => x.Tasks).OrderBy(x => x.Id).ToListAsync(ct)).Select(x => x.ToDto()).ToList());

    /// <summary>Возвращает проект по идентификатору.</summary>
    [HttpGet("{id:int}"), ProducesResponseType(typeof(ProjectDto), 200), ProducesResponseType(404)]
    public async Task<ActionResult<ProjectDto>> Get(int id, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().Include(x => x.Tasks).FirstOrDefaultAsync(x => x.Id == id, ct);
        return project is null ? NotFound() : Ok(project.ToDto());
    }

    /// <summary>Создаёт проект.</summary>
    [HttpPost, ProducesResponseType(typeof(ProjectDto), 201), ProducesResponseType(400)]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto input, CancellationToken ct)
    {
        var project = new Project { Name = input.Name.Trim(), Description = input.Description, CreatedAt = DateTime.UtcNow };
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
        var version = RouteData.Values["version"]?.ToString();
        var location = version is null ? $"/api/projects/{project.Id}" : $"/api/v{version}/projects/{project.Id}";
        return Created(location, project.ToDto());
    }

    /// <summary>Полностью обновляет проект.</summary>
    [HttpPut("{id:int}"), ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, UpdateProjectDto input, CancellationToken ct)
    {
        var project = await db.Projects.FindAsync([id], ct);
        if (project is null) return NotFound();
        project.Name = input.Name.Trim();
        project.Description = input.Description;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Удаляет проект, его задачи и комментарии.</summary>
    [HttpDelete("{id:int}"), ProducesResponseType(204), ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var project = await db.Projects.FindAsync([id], ct);
        if (project is null) return NotFound();
        db.Projects.Remove(project);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
