using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureTodo.Data;

namespace SecureTodo.Pages.Account;

public sealed class LoginModel(SignInManager<AppUser> signIn, UserManager<AppUser> users) : PageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    public sealed class LoginInput
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }
    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await users.FindByEmailAsync(Input.Email.Trim());
        if (user is null) { ModelState.AddModelError(string.Empty, "Неверный логин или пароль."); return Page(); }
        var result = await signIn.PasswordSignInAsync(user, Input.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.RequiresTwoFactor) return RedirectToPage("/Account/LoginWith2fa");
        if (result.IsLockedOut) ModelState.AddModelError(string.Empty, "Аккаунт временно заблокирован на 15 минут.");
        else if (!result.Succeeded) ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
        else return RedirectToPage("/Index");
        return Page();
    }
}
