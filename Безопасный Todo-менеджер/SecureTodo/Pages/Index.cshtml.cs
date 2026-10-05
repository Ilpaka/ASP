using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecureTodo.Data;

namespace SecureTodo.Pages;

public sealed class IndexModel(AppDbContext db) : PageModel
{
    public string DisplayName { get; private set; } = "";
    public int TotalTasks { get; private set; }
    public int CompletedTasks { get; private set; }
    public int ActiveTasks => TotalTasks - CompletedTasks;

    public async Task OnGetAsync()
    {
        if (User.Identity?.IsAuthenticated != true) return;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return;
        DisplayName = await db.Users.Where(x => x.Id == userId).Select(x => x.FullName).FirstOrDefaultAsync()
            ?? User.Identity?.Name ?? "пользователь";
        TotalTasks = await db.Tasks.CountAsync(x => x.UserId == userId);
        CompletedTasks = await db.Tasks.CountAsync(x => x.UserId == userId && x.IsCompleted);
    }
}
