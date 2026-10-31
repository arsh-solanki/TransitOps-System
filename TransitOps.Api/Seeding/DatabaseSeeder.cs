using TransitOps.Api.Repositories.Interfaces;

namespace TransitOps.Api.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedDefaultUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        var demoUsers = new (string FullName, string Email, string Password, int RoleId)[]
        {
            ("Fleet Operations Manager", "manager@transitops.com", "Test@123", 1),
            ("Primary Driver", "driver@transitops.com", "Test@123", 2),
            ("Safety Compliance Officer", "safety@transitops.com", "Test@123", 3),
            ("Financial Controller", "finance@transitops.com", "Test@123", 4)
        };

        foreach (var (fullName, email, password, roleId) in demoUsers)
        {
            try
            {
                var existing = await userRepo.GetByEmailAsync(email);
                if (existing == null)
                {
                    var hash = BCrypt.Net.BCrypt.HashPassword(password);
                    var res = await userRepo.RegisterAsync(fullName, email, hash, roleId);
                    if (res.IsSuccess)
                    {
                        logger.LogInformation("Seeded demo user: {Email} (Role {RoleId})", email, roleId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("Notice: Could not seed user {Email} ({Message}). Database might be initializing.", email, ex.Message);
            }
        }
    }
}

