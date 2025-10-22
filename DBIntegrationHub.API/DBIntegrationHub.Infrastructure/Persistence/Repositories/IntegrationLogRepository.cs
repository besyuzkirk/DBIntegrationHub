using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Infrastructure.Persistence.Repositories;

public class IntegrationLogRepository : Repository<IntegrationLog>, IIntegrationLogRepository
{
    public IntegrationLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<IntegrationLog>> GetByIntegrationIdAsync(
        Guid integrationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(l => l.IntegrationId == integrationId)
            .OrderByDescending(l => l.RunDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<IntegrationLog>> GetRecentLogsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(l => l.RunDate)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}

