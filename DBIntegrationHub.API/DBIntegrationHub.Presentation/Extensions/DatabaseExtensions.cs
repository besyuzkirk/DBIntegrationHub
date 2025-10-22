using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Presentation.Extensions;

public static class DatabaseExtensions
{
    public static async Task<IApplicationBuilder> MigrateDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();

            // Migration'ları uygula
            await context.Database.MigrateAsync();

            // Seed data'yı ekle
            var seeder = new DatabaseSeeder(context, passwordHasher);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Veritabanı migration veya seed işlemi sırasında bir hata oluştu.");
            throw;
        }

        return app;
    }
}

