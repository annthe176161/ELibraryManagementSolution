using ELibraryManagement.Api.Data;
using ELibraryManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELibraryManagement.Api.Seeders
{
    public class AdminUserSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var context = services.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created
                await context.Database.MigrateAsync();

                // Create roles if they don't exist
                string[] roleNames = { "Admin", "Librarian", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Create admin user if it doesn't exist
                var adminUser = await userManager.FindByEmailAsync("admin@elibrary.com");
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = "admin@elibrary.com",
                        FirstName = "System",
                        LastName = "Administrator",
                        PhoneNumber = "0123456789",
                        Address = "System Admin",
                        DateOfBirth = new DateTime(1980, 1, 1),
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine("Admin user created successfully!");
                        Console.WriteLine("Email: admin@elibrary.com");
                        Console.WriteLine("Password: Admin@123");
                    }
                    else
                    {
                        Console.WriteLine("Failed to create admin user:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"- {error.Description}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Admin user already exists.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding admin user: {ex.Message}");
            }
        }
    }
}
