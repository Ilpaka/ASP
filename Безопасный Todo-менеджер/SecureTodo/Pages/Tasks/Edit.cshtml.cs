using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureTodo.Data;

namespace SecureTodo.Pages.Tasks;

[Authorize]
public sealed class EditModel(AppDbContext db, IAuthorizationService authorization) : PageModel
{
    [BindProperty] public EditInput Input { get; set; } = new();
    public sealed class EditInput
    {
        [Required, StringLength(120, MinimumLength = 3)] public string Title { get; set; } = "";
        [StringLength(2000)] public string? Description { get; set; }
        public bool IsCompleted { get; set; }
    }
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!(await authorization.AuthorizeAsync(User, task, "CanEditTask")).Succeeded) return Forbid();
        Input = new EditInput { Title = task.Title, Description = task.Description, IsCompleted = task.IsCompleted };
        return Page();
    }
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var task = await db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        if (!(await authorization.AuthorizeAsync(User, task, "CanEditTask")).Succeeded) return Forbid();
        if (!ModelState.IsValid) return Page();
        task.Title = Input.Title.Trim(); task.Description = Input.Description; task.IsCompleted = Input.IsCompleted;
        await db.SaveChangesAsync();
        TempData["Message"] = "Задача обновлена.";
        return RedirectToPage("/Tasks/Index");
    }
}
