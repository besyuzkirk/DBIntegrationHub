using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Shared;
using DBIntegrationHub.Infrastructure.Persistence.EntityConfigurations;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Integration> Integrations => Set<Integration>();
    public DbSet<Mapping> Mappings => Set<Mapping>();
    public DbSet<IntegrationLog> IntegrationLogs => Set<IntegrationLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ScheduledJob> ScheduledJobs => Set<ScheduledJob>();
    
    // Data Protection Keys için
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ConnectionConfiguration());
        modelBuilder.ApplyConfiguration(new IntegrationConfiguration());
        modelBuilder.ApplyConfiguration(new MappingConfiguration());
        modelBuilder.ApplyConfiguration(new IntegrationLogConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduledJobConfiguration());
        modelBuilder.ApplyConfiguration(new DataProtectionKeyConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Timestamp'leri otomatik güncelle
        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // CreatedAt zaten constructor'da set ediliyor
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
