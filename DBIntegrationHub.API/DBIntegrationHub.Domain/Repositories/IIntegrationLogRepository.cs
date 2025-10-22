using DBIntegrationHub.Domain.Entities;

namespace DBIntegrationHub.Domain.Repositories;

public interface IIntegrationLogRepository : IRepository<IntegrationLog>
{
    Task<IEnumerable<IntegrationLog>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IntegrationLog>> GetRecentLogsAsync(int count, CancellationToken cancellationToken = default);
}

