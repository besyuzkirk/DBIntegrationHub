namespace DBIntegrationHub.Application.Abstractions.Data;

public interface IConnectionTester
{
    Task<ConnectionTestResult> TestConnectionAsync(
        string databaseType,
        string connectionString,
        CancellationToken cancellationToken = default);
}

public record ConnectionTestResult(
    bool IsSuccess,
    string Message,
    int? ResponseTimeMs = null);

