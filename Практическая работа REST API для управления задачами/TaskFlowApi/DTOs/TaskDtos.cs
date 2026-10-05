using System.ComponentModel.DataAnnotations;
using TaskFlowApi.Entities;
using TaskStatus = TaskFlowApi.Entities.TaskStatus;

namespace TaskFlowApi.DTOs;

/// <summary>Задача по контракту v1.</summary>
public sealed record TaskItemDto(int Id, string Title, string? Description, TaskStatus Status, int ProjectId, int? AssignedToId, string? AssignedUsername, DateTime? DueDate);
/// <summary>Задача по контракту v2: приоритет вместо описания.</summary>
public sealed record TaskItemV2Dto(int Id, string Title, TaskStatus Status, Priority Priority, int ProjectId, int? AssignedToId, string? AssignedUsername, DateTime? DueDate);

/// <summary>Создание задачи v1; отсутствующий статус означает ToDo.</summary>
public sealed class CreateTaskDto : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 5)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    [Range(1, int.MaxValue)] public int ProjectId { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DueDate is not null && DueDate <= DateTime.UtcNow)
            yield return new ValidationResult("DueDate должна быть в будущем.", [nameof(DueDate)]);
    }
}

/// <summary>Полная замена задачи v1; статус обязателен.</summary>
public sealed class UpdateTaskDto : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 5)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Required] public TaskStatus? Status { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DueDate is not null && DueDate <= DateTime.UtcNow)
            yield return new ValidationResult("DueDate должна быть в будущем.", [nameof(DueDate)]);
    }
}

/// <summary>Создание задачи v2.</summary>
public sealed class CreateTaskV2Dto : CreateTaskDtoBase
{
    public Priority Priority { get; set; } = Priority.Medium;
}

public class CreateTaskDtoBase : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 5)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    [Range(1, int.MaxValue)] public int ProjectId { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DueDate is not null && DueDate <= DateTime.UtcNow)
            yield return new ValidationResult("DueDate должна быть в будущем.", [nameof(DueDate)]);
    }
}

/// <summary>Полная замена задачи v2.</summary>
public sealed class UpdateTaskV2Dto : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 5)] public string Title { get; set; } = "";
    public string? Description { get; set; }
    [Required] public TaskStatus? Status { get; set; }
    [Required] public Priority? Priority { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DueDate is not null && DueDate <= DateTime.UtcNow)
            yield return new ValidationResult("DueDate должна быть в будущем.", [nameof(DueDate)]);
    }
}

public sealed class UpdateTaskStatusDto
{
    [Required] public TaskStatus? Status { get; set; }
}

/// <summary>Параметры фильтрации задач v2.</summary>
public sealed class TaskQuery
{
    /// <summary>Номер страницы, начиная с 1.</summary>
    public int Page { get; set; } = 1;
    /// <summary>Число записей, от 1 до 50.</summary>
    public int PageSize { get; set; } = 10;
    /// <summary>Фильтр: ToDo, InProgress или Done.</summary>
    public TaskStatus? Status { get; set; }
    /// <summary>Фильтр: Low, Medium или High.</summary>
    public Priority? Priority { get; set; }
    /// <summary>Сортировка: title, dueDate, createdAt.</summary>
    public string SortBy { get; set; } = "createdAt";
    /// <summary>Направление: asc или desc.</summary>
    public string SortDir { get; set; } = "desc";
    /// <summary>Поиск подстроки в названии.</summary>
    public string? Search { get; set; }
}

public sealed record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
