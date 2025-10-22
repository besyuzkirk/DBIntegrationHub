using DBIntegrationHub.Domain.Entities;

namespace DBIntegrationHub.Domain.Repositories;

public interface IMappingRepository : IRepository<Mapping>
{
    Task<IEnumerable<Mapping>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task DeleteByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
}

