using Microsoft.AspNetCore.Identity;

namespace SecurePortal.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(
        IServiceProvider services)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        [
            "Admin",
            "Manager",
            "User"
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }
    }
}