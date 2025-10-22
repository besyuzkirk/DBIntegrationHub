using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Infrastructure.Persistence.Repositories;

public class MappingRepository : Repository<Mapping>, IMappingRepository
{
    public MappingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Mapping>> GetByIntegrationIdAsync(
        Guid integrationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(m => m.IntegrationId == integrationId)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByIntegrationIdAsync(
        Guid integrationId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await _dbSet
            .Where(m => m.IntegrationId == integrationId)
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(mappings);
    }
}

