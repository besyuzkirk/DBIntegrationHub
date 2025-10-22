using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.ScheduledJobs.Queries.GetAllScheduledJobs;

public record GetAllScheduledJobsQuery : IQuery<IEnumerable<ScheduledJobDto>>;

public record ScheduledJobDto(
    Guid Id,
    string Name,
    string Description,
    string CronExpression,
    bool IsActive,
    Guid? IntegrationId,
    string? IntegrationName,
    Guid? GroupId,
    DateTime? LastRunAt,
    DateTime? NextRunAt,
    int TotalRuns,
    int SuccessfulRuns,
    int FailedRuns,
    DateTime CreatedAt
);

