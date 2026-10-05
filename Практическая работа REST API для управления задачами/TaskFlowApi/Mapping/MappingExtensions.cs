using TaskFlowApi.DTOs;
using TaskFlowApi.Entities;

namespace TaskFlowApi.Mapping;

public static class MappingExtensions
{
    public static ProjectDto ToDto(this Project project) => new(project.Id, project.Name, project.Description, project.CreatedAt, project.Tasks.Count);
    public static TaskItemDto ToV1Dto(this TaskItem task) => new(task.Id, task.Title, task.Description, task.Status, task.ProjectId, task.AssignedToId, task.AssignedTo?.Username, task.DueDate);
    public static TaskItemV2Dto ToV2Dto(this TaskItem task) => new(task.Id, task.Title, task.Status, task.Priority, task.ProjectId, task.AssignedToId, task.AssignedTo?.Username, task.DueDate);
    public static CommentDto ToDto(this Comment comment) => new(comment.Id, comment.TaskItemId, comment.Author.Username, comment.Content, comment.CreatedAt);
}
