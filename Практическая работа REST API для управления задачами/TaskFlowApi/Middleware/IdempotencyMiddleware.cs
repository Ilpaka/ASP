using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.Entities;

namespace TaskFlowApi.Middleware;

/// <summary>Сохраняет ответ POST v2 и предотвращает повторное создание задачи.</summary>
public sealed class IdempotencyMiddleware(RequestDelegate next)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !string.Equals(context.Request.Path.Value, "/api/v2/tasks", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (!Guid.TryParse(context.Request.Headers["X-Idempotency-Key"].ToString(), out var guid))
        {
            await WriteProblem(context, 400, "Invalid idempotency key", "Требуется X-Idempotency-Key в формате GUID.");
            return;
        }

        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync(context.RequestAborted);
        context.Request.Body.Position = 0;
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
        var key = guid.ToString("D");

        await Gate.WaitAsync(context.RequestAborted);
        try
        {
            var existing = await db.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key, context.RequestAborted);
            if (existing is not null)
            {
                if (existing.RequestBodyHash != hash)
                {
                    await WriteProblem(context, 409, "Idempotency key conflict", "Idempotency key reused with different body");
                    return;
                }
                context.Response.StatusCode = existing.StatusCode;
                context.Response.ContentType = "application/json";
                if (existing.Location is not null) context.Response.Headers.Location = existing.Location;
                await context.Response.WriteAsync(existing.ResponseBody, context.RequestAborted);
                return;
            }

            await using var transaction = await db.Database.BeginTransactionAsync(context.RequestAborted);
            var originalBody = context.Response.Body;
            await using var buffer = new MemoryStream();
            context.Response.Body = buffer;
            try
            {
                await next(context);
                buffer.Position = 0;
                var responseBody = await new StreamReader(buffer, Encoding.UTF8, leaveOpen: true).ReadToEndAsync(context.RequestAborted);
                if (context.Response.StatusCode == 201)
                {
                    db.IdempotencyRecords.Add(new IdempotencyRecord
                    {
                        Key = key, RequestBodyHash = hash, ResponseBody = responseBody,
                        StatusCode = 201, Location = context.Response.Headers.Location.ToString(), CreatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync(context.RequestAborted);
                    await transaction.CommitAsync(context.RequestAborted);
                }
                else await transaction.RollbackAsync(context.RequestAborted);
                context.Response.Body = originalBody;
                await context.Response.WriteAsync(responseBody, context.RequestAborted);
            }
            finally { context.Response.Body = originalBody; }
        }
        finally { Gate.Release(); }
    }

    private static Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = status, Title = title, Detail = detail, Instance = context.Request.Path
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
