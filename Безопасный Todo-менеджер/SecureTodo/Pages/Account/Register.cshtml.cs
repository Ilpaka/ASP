using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureTodo.Data;

namespace SecureTodo.Pages.Account;

public sealed class RegisterModel(UserManager<AppUser> users, SignInManager<AppUser> signIn) : PageModel
{
    [BindProperty] public RegisterInput Input { get; set; } = new();
    public sealed class RegisterInput
    {
        [Required, StringLength(100)] public string FullName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; set; } = "";
        [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    }
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = new AppUser { UserName = Input.Email.Trim(), Email = Input.Email.Trim(), FullName = Input.FullName.Trim() };
        var result = await users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
        var roleResult = await users.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded) throw new InvalidOperationException("Не удалось назначить роль User.");
        await signIn.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Index");
    }
}
