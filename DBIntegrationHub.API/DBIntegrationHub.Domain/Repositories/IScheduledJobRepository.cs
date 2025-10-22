using DBIntegrationHub.Domain.Entities;

namespace DBIntegrationHub.Domain.Repositories;

public interface IScheduledJobRepository : IRepository<ScheduledJob>
{
    Task<IEnumerable<ScheduledJob>> GetActiveJobsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ScheduledJob>> GetByIntegrationIdAsync(Guid integrationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ScheduledJob>> GetByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<ScheduledJob?> GetByHangfireJobIdAsync(string hangfireJobId, CancellationToken cancellationToken = default);
}

