using Microsoft.AspNetCore.Identity;

namespace R2.Data.DataSeed
{
    public static class SeedRoles
    {
        public static async Task InitializeRoles(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            string[] roleNames = { "Citoyen", "Administrateur", "Super-Administrateur", "Modérateur" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    // Création du rôle s'il n'existe pas
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }
    }
}