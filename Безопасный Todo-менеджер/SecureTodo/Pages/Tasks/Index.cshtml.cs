using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecureTodo.Data;

namespace SecureTodo.Pages.Tasks;

[Authorize]
public sealed class IndexModel(AppDbContext db, IAuthorizationService authorization) : PageModel
{
    public List<TaskItem> Items { get; private set; } = [];
    public async Task OnGetAsync()
    {
        var query = db.Tasks.AsNoTracking().Include(x => x.User).AsQueryable();
        if (!User.IsInRole("Admin"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            query = query.Where(x => x.UserId == userId);
        }
        Items = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<IActionResult> OnPostCompleteAsync(int id)
    {
        if (User.Identity?.IsAuthenticated != true) return Unauthorized();
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!(await authorization.AuthorizeAsync(User, task, "CanEditTask")).Succeeded) return Forbid();
        task.IsCompleted = true;
        await db.SaveChangesAsync();
        return new JsonResult(new { isCompleted = true });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (User.Identity?.IsAuthenticated != true) return Challenge();
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!(await authorization.AuthorizeAsync(User, task, "CanEditTask")).Succeeded) return Forbid();
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        TempData["Message"] = "Задача удалена.";
        return RedirectToPage();
    }
}
