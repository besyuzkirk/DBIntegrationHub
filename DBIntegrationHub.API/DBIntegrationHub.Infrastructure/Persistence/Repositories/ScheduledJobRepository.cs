using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Infrastructure.Persistence.Repositories;

public class ScheduledJobRepository : Repository<ScheduledJob>, IScheduledJobRepository
{
    public ScheduledJobRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ScheduledJob>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(j => j.Integration)
            .Where(j => j.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduledJob>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(j => j.IntegrationId == integrationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduledJob>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(j => j.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduledJob?> GetByHangfireJobIdAsync(string hangfireJobId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(j => j.HangfireJobId == hangfireJobId, cancellationToken);
    }
}

