using DBIntegrationHub.Application.Abstractions.Caching;
using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Email;
using DBIntegrationHub.Application.Abstractions.Jobs;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Infrastructure.Caching;
using DBIntegrationHub.Infrastructure.Data;
using DBIntegrationHub.Infrastructure.Data.TypeMapping;
using DBIntegrationHub.Infrastructure.Email;
using DBIntegrationHub.Infrastructure.Jobs;
using DBIntegrationHub.Infrastructure.Persistence;
using DBIntegrationHub.Infrastructure.Persistence.Repositories;
using DBIntegrationHub.Infrastructure.Security;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DBIntegrationHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Data Protection API - Connection string şifreleme için
        services.AddDataProtection()
            .SetApplicationName("DBIntegrationHub")
            .PersistKeysToDbContext<ApplicationDbContext>();
        
        // Repositories
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IIntegrationRepository, IntegrationRepository>();
        services.AddScoped<IMappingRepository, MappingRepository>();
        services.AddScoped<IIntegrationLogRepository, IntegrationLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IScheduledJobRepository, ScheduledJobRepository>();
        
        // Data Services
        services.AddScoped<IConnectionTester, ConnectionTester>();
        services.AddScoped<IQueryPreviewService, QueryPreviewService>();
        services.AddScoped<IIntegrationRunner, IntegrationRunner>();
        services.AddScoped<ITypeMapper, TypeMapperService>();
        services.AddScoped<ISchemaService, SchemaService>();

        // Caching - Memory Cache için
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        // Email
        services.AddScoped<IEmailService, EmailService>();
        
        // Security
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // HttpContextAccessor - CurrentUserService için gerekli
        services.AddHttpContextAccessor();

        // Hangfire - Job Scheduling
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))));

        services.AddHangfireServer();
        
        // Job Scheduler
        services.AddScoped<IIntegrationJobScheduler, IntegrationJobScheduler>();
        services.AddScoped<IntegrationJobExecutor>();

        return services;
    }
}

