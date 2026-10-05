using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SecureTodo.Data;

namespace SecureTodo.Services;

public sealed record TokenPair(string AccessToken, string RefreshToken, DateTime AccessExpiresAt, DateTime RefreshExpiresAt);

public sealed class JwtTokenService(IConfiguration configuration, UserManager<AppUser> users)
{
    public async Task<TokenPair> CreatePairAsync(AppUser user)
    {
        var now = DateTime.UtcNow;
        var accessExpiry = now.AddMinutes(15);
        var refreshExpiry = now.AddDays(7);
        var roles = await users.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"], audience: configuration["Jwt:Audience"],
            claims: claims, notBefore: now, expires: accessExpiry,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new TokenPair(new JwtSecurityTokenHandler().WriteToken(token), Guid.NewGuid().ToString("N"), accessExpiry, refreshExpiry);
    }
}
