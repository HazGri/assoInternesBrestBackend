using AssoInternesBrest.API.Entities;
using AssoInternesBrest.API.Services;
using Microsoft.EntityFrameworkCore;

namespace AssoInternesBrest.API.Data
{
    public static class SeedData
    {
        public static async Task SeedInitialAdminAsync(IServiceProvider services, IConfiguration configuration)
        {
            string? email = configuration["InitialAdmin:Email"];
            string? password = configuration["InitialAdmin:Password"];
            string firstName = configuration["InitialAdmin:FirstName"] ?? "Admin";
            string lastName = configuration["InitialAdmin:LastName"] ?? "Système";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            using IServiceScope scope = services.CreateScope();
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            IPasswordService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

            bool anyAdmin = await context.Users.AnyAsync(u => u.Role == UserRole.Admin);
            if (anyAdmin)
            {
                return;
            }

            User admin = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = passwordService.HashPassword(password),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
