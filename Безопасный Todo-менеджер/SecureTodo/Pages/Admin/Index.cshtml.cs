using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SecureTodo.Data;

namespace SecureTodo.Pages.Admin;

[Authorize(Roles = "Admin")]
public sealed class IndexModel(UserManager<AppUser> users, AppDbContext db) : PageModel
{
    public sealed record UserRow(string Id, string FullName, string Email, string Role);
    public List<UserRow> Users { get; private set; } = [];
    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
    {
        if (newRole is not ("User" or "Author"))
        {
            ModelState.AddModelError(string.Empty, "Можно назначить только User или Author.");
            await LoadAsync(); return Page();
        }
        var target = await users.FindByIdAsync(userId);
        if (target is null) return NotFound();
        var oldRoles = await users.GetRolesAsync(target);
        if (oldRoles.Contains("Admin") || target.Id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            return BadRequest("Нельзя снять роль администратора с себя или другого администратора.");
        await using var transaction = await db.Database.BeginTransactionAsync();
        var remove = await users.RemoveFromRolesAsync(target, oldRoles);
        if (!remove.Succeeded) throw new InvalidOperationException("Не удалось убрать старую роль.");
        var add = await users.AddToRoleAsync(target, newRole);
        if (!add.Succeeded) throw new InvalidOperationException("Не удалось назначить новую роль.");
        await transaction.CommitAsync();
        TempData["Message"] = $"Роль пользователя {target.Email} изменена на {newRole}.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var all = await users.Users.OrderBy(x => x.Email).ToListAsync();
        foreach (var user in all)
        {
            var roles = await users.GetRolesAsync(user);
            Users.Add(new UserRow(user.Id, user.FullName ?? "Без имени", user.Email ?? "", roles.FirstOrDefault() ?? "User"));
        }
    }
}
