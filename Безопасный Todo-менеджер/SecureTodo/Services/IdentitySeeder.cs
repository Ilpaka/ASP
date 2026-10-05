using Microsoft.AspNetCore.Identity;
using SecureTodo.Data;

namespace SecureTodo.Services;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, IHostEnvironment environment)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Author", "User" })
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded) throw new InvalidOperationException($"Не удалось создать роль {role}.");
            }

        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrEmpty(password))
        {
            if (environment.IsDevelopment()) throw new InvalidOperationException("Нужен пароль администратора в SeedAdmin:Password.");
            return;
        }
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var admin = await users.FindByEmailAsync("admin@todo.com");
        if (admin is null)
        {
            admin = new AppUser { UserName = "admin@todo.com", Email = "admin@todo.com", FullName = "Администратор", EmailConfirmed = true };
            var created = await users.CreateAsync(admin, password);
            if (!created.Succeeded) throw new InvalidOperationException("Не удалось создать администратора: " + string.Join(", ", created.Errors.Select(x => x.Description)));
        }
        if (!await users.IsInRoleAsync(admin, "Admin")) await users.AddToRoleAsync(admin, "Admin");
    }
}
