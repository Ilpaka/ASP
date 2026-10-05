using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureTodo.Data;

namespace SecureTodo.Pages.Account;

public sealed class LoginWith2faModel(SignInManager<AppUser> signIn) : PageModel
{
    [BindProperty, Required, RegularExpression("^[0-9]{6}$")] public string Code { get; set; } = "";
    public async Task<IActionResult> OnGetAsync()
        => await signIn.GetTwoFactorAuthenticationUserAsync() is null ? RedirectToPage("/Account/Login") : Page();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var result = await signIn.TwoFactorSignInAsync(TokenOptions.DefaultAuthenticatorProvider, Code, false, false);
        if (result.Succeeded) return RedirectToPage("/Index");
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Аккаунт временно заблокирован." : "Неверный код подтверждения.");
        return Page();
    }
}
