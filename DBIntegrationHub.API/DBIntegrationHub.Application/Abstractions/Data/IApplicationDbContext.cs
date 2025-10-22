using DBIntegrationHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Connection> Connections { get; }
    DbSet<Integration> Integrations { get; }
    DbSet<Mapping> Mappings { get; }
    DbSet<IntegrationLog> IntegrationLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

