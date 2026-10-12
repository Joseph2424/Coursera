using LogiTrack.Security;
using Microsoft.AspNetCore.Identity;

namespace LogiTrack.Data;

public sealed class IdentitySeeder(
    RoleManager<IdentityRole> roleManager,
    UserManager<IdentityUser> userManager,
    IConfiguration configuration)
{
    public async Task SeedAsync()
    {
        foreach (var role in new[] { AppRoles.User, AppRoles.Admin })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                    throw new InvalidOperationException($"Unable to create the {role} role.");
            }
        }

        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
            return;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Configure both BootstrapAdmin:Email and BootstrapAdmin:Password.");

        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var userResult = await userManager.CreateAsync(admin, password);
            if (!userResult.Succeeded)
                throw new InvalidOperationException("Unable to create the bootstrap admin. Check the password policy and configuration.");
        }

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException("Unable to assign the bootstrap admin role.");
        }
    }
}