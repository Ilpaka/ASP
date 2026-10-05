using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TaskFlowApi.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Ошибка при обработке {Path}", context.Request.Path);
        var (status, title, detail) = exception switch
        {
            NotFoundException => (404, "Resource not found", exception.Message),
            BusinessRuleException => (422, "Business rule violation", exception.Message),
            _ => (500, "Internal Server Error", "Внутренняя ошибка сервера")
        };
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = status, Title = title, Detail = detail, Instance = context.Request.Path
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web)), cancellationToken);
        return true;
    }
}
