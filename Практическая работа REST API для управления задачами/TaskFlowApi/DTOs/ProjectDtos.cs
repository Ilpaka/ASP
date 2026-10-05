using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.DTOs;

/// <summary>Проект с вычисленным количеством задач.</summary>
public sealed record ProjectDto(int Id, string Name, string? Description, DateTime CreatedAt, int TaskCount);

/// <summary>Данные нового проекта.</summary>
public sealed class CreateProjectDto
{
    /// <summary>Название проекта, от 3 до 100 символов.</summary>
    [Required, StringLength(100, MinimumLength = 3)] public string Name { get; set; } = "";
    /// <summary>Необязательное описание.</summary>
    public string? Description { get; set; }
}

/// <summary>Полная замена проекта.</summary>
public sealed class UpdateProjectDto : CreateProjectDtoBase { }

public class CreateProjectDtoBase
{
    [Required, StringLength(100, MinimumLength = 3)] public string Name { get; set; } = "";
    public string? Description { get; set; }
}
