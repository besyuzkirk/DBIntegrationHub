using DBIntegrationHub.Domain.Entities;

namespace DBIntegrationHub.Domain.Repositories;

public interface IIntegrationRepository : IRepository<Integration>
{
    Task<Integration?> GetByIdWithConnectionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Integration>> GetAllWithConnectionsAsync(CancellationToken cancellationToken = default);
}

