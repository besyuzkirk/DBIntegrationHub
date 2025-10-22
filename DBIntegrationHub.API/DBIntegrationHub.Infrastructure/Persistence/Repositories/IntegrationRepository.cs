using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Infrastructure.Persistence.Repositories;

public class IntegrationRepository : Repository<Integration>, IIntegrationRepository
{
    public IntegrationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Integration?> GetByIdWithConnectionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.SourceConnection)
            .Include(i => i.TargetConnection)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Integration>> GetAllWithConnectionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(i => i.SourceConnection)
            .Include(i => i.TargetConnection)
            .ToListAsync(cancellationToken);
    }
}

