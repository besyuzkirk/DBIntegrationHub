namespace DBIntegrationHub.Application.IntegrationLogs.Queries.Dtos;

public record IntegrationLogDto(
    Guid Id,
    Guid IntegrationId,
    DateTime RunDate,
    bool Success,
    string? Message,
    int RowCount,
    long DurationMs,
    string? ErrorDetails);

