namespace DBIntegrationHub.Application.Abstractions.Data;

public interface IIntegrationRunner
{
    Task<IntegrationRunResult> RunAsync(
        string sourceDatabaseType,
        string sourceConnectionString,
        string sourceQuery,
        string targetDatabaseType,
        string targetConnectionString,
        string targetQuery,
        Dictionary<string, string>? columnMappings = null,
        CancellationToken cancellationToken = default);
}

public record IntegrationRunResult(
    bool Success,
    int RowsAffected,
    long DurationMs,
    string? Message = null,
    string? Error = null);

