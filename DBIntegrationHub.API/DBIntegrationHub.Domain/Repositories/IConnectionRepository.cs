using DBIntegrationHub.Domain.Entities;

namespace DBIntegrationHub.Domain.Repositories;

public interface IConnectionRepository : IRepository<Connection>
{
    Task<Connection?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Connection>> GetActiveConnectionsAsync(CancellationToken cancellationToken = default);
}

