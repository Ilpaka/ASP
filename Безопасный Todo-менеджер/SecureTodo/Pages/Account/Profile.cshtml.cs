using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecureTodo.Data;

namespace SecureTodo.Pages.Account;

[Authorize]
public sealed class ProfileModel(UserManager<AppUser> users, SignInManager<AppUser> signIn) : PageModel
{
    public bool IsEnabled { get; private set; }
    public string? AuthenticatorKey { get; private set; }
    public string? OtpAuthUri { get; private set; }
    [BindProperty, Required, RegularExpression("^[0-9]{6}$")] public string Code { get; set; } = "";

    private async Task<AppUser> CurrentUserAsync() => await users.GetUserAsync(User) ?? throw new InvalidOperationException("Пользователь не найден.");
    public async Task OnGetAsync() => IsEnabled = await users.GetTwoFactorEnabledAsync(await CurrentUserAsync());

    public async Task<IActionResult> OnPostBegin2faAsync()
    {
        var user = await CurrentUserAsync();
        if (await users.GetTwoFactorEnabledAsync(user)) return RedirectToPage();
        await users.ResetAuthenticatorKeyAsync(user);
        await signIn.RefreshSignInAsync(user);
        await ShowSetupAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostEnable2faAsync()
    {
        var user = await CurrentUserAsync();
        if (!ModelState.IsValid) { await ShowSetupAsync(user); return Page(); }
        var valid = await users.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, Code);
        if (!valid)
        {
            ModelState.AddModelError(nameof(Code), "Код не подошёл.");
            await ShowSetupAsync(user);
            return Page();
        }
        await users.SetTwoFactorEnabledAsync(user, true);
        await signIn.RefreshSignInAsync(user);
        TempData["Message"] = "Двухфакторная аутентификация включена.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDisable2faAsync()
    {
        var user = await CurrentUserAsync();
        await users.SetTwoFactorEnabledAsync(user, false);
        await users.ResetAuthenticatorKeyAsync(user);
        await signIn.RefreshSignInAsync(user);
        TempData["Message"] = "Двухфакторная аутентификация отключена.";
        return RedirectToPage();
    }

    private async Task ShowSetupAsync(AppUser user)
    {
        IsEnabled = false;
        AuthenticatorKey = await users.GetAuthenticatorKeyAsync(user);
        var issuer = Uri.EscapeDataString("SecureTodo");
        var account = Uri.EscapeDataString(user.Email ?? user.UserName ?? user.Id);
        OtpAuthUri = $"otpauth://totp/{issuer}:{account}?secret={AuthenticatorKey}&issuer={issuer}&digits=6";
    }
}
