using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Dashboard.Queries;

public record GetDashboardStatisticsQuery : IQuery<DashboardStatisticsResponse>;

public record DashboardStatisticsResponse(
    int TotalConnections,
    int ActiveConnections,
    int InactiveConnections,
    int TotalIntegrations,
    int TotalMappings,
    int TotalLogs,
    int TodayLogs,
    int SuccessfulLogsToday,
    int FailedLogsToday,
    decimal SuccessRateToday,
    decimal OverallSuccessRate
);

