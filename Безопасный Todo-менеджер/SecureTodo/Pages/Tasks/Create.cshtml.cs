using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureTodo.Data;

namespace SecureTodo.Pages.Tasks;

[Authorize]
public sealed class CreateModel(AppDbContext db) : PageModel
{
    [BindProperty] public TaskInput Input { get; set; } = new();
    public sealed class TaskInput
    {
        [Required, StringLength(120, MinimumLength = 3)] public string Title { get; set; } = "";
        [StringLength(2000)] public string? Description { get; set; }
    }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Challenge();
        db.Tasks.Add(new TaskItem { Title = Input.Title.Trim(), Description = Input.Description, UserId = userId, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        TempData["Message"] = "Задача создана.";
        return RedirectToPage("/Tasks/Index");
    }
}
