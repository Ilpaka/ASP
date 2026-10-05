using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SecureTodo.Data;

namespace SecureTodo.Security;

public sealed class CanEditTaskRequirement : IAuthorizationRequirement;

public sealed class CanEditTaskHandler : AuthorizationHandler<CanEditTaskRequirement, TaskItem>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        CanEditTaskRequirement requirement, TaskItem resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null && (resource.UserId == userId || context.User.IsInRole("Admin")))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
