using Microsoft.EntityFrameworkCore;
using ELibraryManagement.Api.Data;

namespace ELibraryManagement.Api.Data.Seeders
{
    public static class UserStatusSeeder
    {
        public static async Task SeedUserStatusAsync(ApplicationDbContext context)
        {
            // Update all existing users to have IsActive = true
            await context.Database.ExecuteSqlRawAsync("UPDATE AspNetUsers SET IsActive = 1 WHERE IsActive = 0");
        }
    }
}