using BlindMatchPAS.Helpers;
using BlindMatchPAS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await SeedAdminAsync(userManager);
        await SeedResearchAreasAsync(context);
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@blindmatchpas.local";
        const string adminPassword = "Admin@12345";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is not null)
        {
            return;
        }

        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Module Leader",
            EmailConfirmed = true,
            IsActive = true,
            RoleDisplayName = "Admin / Module Leader"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
        }
    }

    private static async Task SeedResearchAreasAsync(ApplicationDbContext context)
    {
        if (await context.ResearchAreas.AnyAsync())
        {
            return;
        }

        var areas = new[]
        {
            new ResearchArea { Name = "Artificial Intelligence", Description = "Machine learning, computer vision, intelligent systems." },
            new ResearchArea { Name = "Cyber Security", Description = "Security engineering, privacy, digital forensics." },
            new ResearchArea { Name = "Software Engineering", Description = "Architecture, quality assurance, DevOps, testing." },
            new ResearchArea { Name = "Data Science", Description = "Analytics, data engineering, visualization, mining." },
            new ResearchArea { Name = "Human Computer Interaction", Description = "Usability, accessibility, user experience research." }
        };

        await context.ResearchAreas.AddRangeAsync(areas);
        await context.SaveChangesAsync();
    }
}
