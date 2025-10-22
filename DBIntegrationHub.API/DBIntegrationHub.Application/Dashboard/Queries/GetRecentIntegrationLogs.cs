using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Dashboard.Queries;

public record GetRecentIntegrationLogsQuery(int Count = 10) : IQuery<List<RecentIntegrationLogDto>>;

public record RecentIntegrationLogDto(
    Guid Id,
    Guid IntegrationId,
    string IntegrationName,
    DateTime RunDate,
    bool Success,
    int RowCount,
    long DurationMs,
    string? Message,
    string? ErrorDetails
);

