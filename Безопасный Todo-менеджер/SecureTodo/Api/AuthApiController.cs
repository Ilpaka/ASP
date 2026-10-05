using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTodo.Data;
using SecureTodo.Services;

namespace SecureTodo.Api;

[ApiController]
[Route("api/auth")]
public sealed class AuthApiController(SignInManager<AppUser> signIn, UserManager<AppUser> users,
    JwtTokenService tokens, AppDbContext db) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenPair), 200), ProducesResponseType(400), ProducesResponseType(401), ProducesResponseType(423)]
    public async Task<IActionResult> Login(ApiLoginRequest input)
    {
        var user = await users.FindByEmailAsync(input.Email.Trim());
        if (user is null) return Unauthorized(new { message = "Неверный логин или пароль." });
        var result = await signIn.CheckPasswordSignInAsync(user, input.Password, lockoutOnFailure: true);
        if (result.IsLockedOut) return StatusCode(423, new { message = "Аккаунт временно заблокирован." });
        if (!result.Succeeded) return Unauthorized(new { message = "Неверный логин или пароль." });
        if (await users.GetTwoFactorEnabledAsync(user))
        {
            var code = input.TwoFactorCode?.Replace(" ", "").Replace("-", "");
            if (code is null || !await users.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code))
            {
                await users.AccessFailedAsync(user);
                if (await users.IsLockedOutAsync(user))
                    return StatusCode(423, new { message = "Аккаунт временно заблокирован." });
                return Unauthorized(new { message = "Нужен верный код двухфакторной аутентификации." });
            }
            await users.ResetAccessFailedCountAsync(user);
        }
        var pair = await tokens.CreatePairAsync(user);
        db.RefreshTokens.Add(new RefreshToken { Token = pair.RefreshToken, UserId = user.Id,
            CreatedAt = DateTime.UtcNow, ExpiresAt = pair.RefreshExpiresAt });
        await db.SaveChangesAsync();
        return Ok(pair);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenPair), 200), ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(RefreshRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.RefreshToken)) return Unauthorized();
        var old = await db.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == input.RefreshToken);
        if (old is null || old.RevokedAt is not null || old.ExpiresAt <= DateTime.UtcNow) return Unauthorized();
        old.RevokedAt = DateTime.UtcNow;
        var pair = await tokens.CreatePairAsync(old.User);
        db.RefreshTokens.Add(new RefreshToken { Token = pair.RefreshToken, UserId = old.UserId,
            CreatedAt = DateTime.UtcNow, ExpiresAt = pair.RefreshExpiresAt });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return Unauthorized(); }
        return Ok(pair);
    }

    [HttpPost("logout")]
    [ProducesResponseType(204), ProducesResponseType(401)]
    public async Task<IActionResult> Logout(RefreshRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.RefreshToken)) return Unauthorized();
        var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == input.RefreshToken);
        if (token is null || token.RevokedAt is not null) return Unauthorized();
        token.RevokedAt = DateTime.UtcNow;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return Unauthorized(); }
        return NoContent();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("logout-all")]
    [ProducesResponseType(204), ProducesResponseType(401)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        await db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(x => x.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
        return NoContent();
    }
}
