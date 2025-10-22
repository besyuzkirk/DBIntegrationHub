using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Dashboard.Queries;

public record GetIntegrationActivityQuery(int Days = 7) : IQuery<List<IntegrationActivityDto>>;

public record IntegrationActivityDto(
    DateTime Date,
    int TotalRuns,
    int SuccessfulRuns,
    int FailedRuns,
    decimal SuccessRate
);

