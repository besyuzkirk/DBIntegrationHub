using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // Rolleri oluştur
        await SeedRolesAsync();
        
        // Admin kullanıcısını oluştur
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            (Name: Role.RoleNames.Admin, Description: "Sistem yöneticisi - Tüm yetkilere sahip"),
            (Name: Role.RoleNames.User, Description: "Normal kullanıcı - Standart yetkiler"),
            (Name: Role.RoleNames.Viewer, Description: "Görüntüleyici - Sadece okuma yetkisi")
        };

        foreach (var (name, description) in roles)
        {
            var exists = await _context.Roles.AnyAsync(r => r.Name == name);
            if (!exists)
            {
                var role = Role.Create(name, description);
                await _context.Roles.AddAsync(role);
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminUsername = "admin";
        const string adminEmail = "admin@dbintegrationhub.com";
        const string adminPassword = "admin";

        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == adminUsername);

        if (existingAdmin == null)
        {
            var passwordHash = _passwordHasher.HashPassword(adminPassword);
            var adminUser = User.Create(adminUsername, adminEmail, passwordHash);
            
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();

            // Admin rolünü ata
            var adminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == Role.RoleNames.Admin);

            if (adminRole != null)
            {
                var userRole = UserRole.Create(adminUser.Id, adminRole.Id);
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }
        }
    }
}

