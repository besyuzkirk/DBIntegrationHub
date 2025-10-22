using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Domain.Entities;

public class IntegrationLog : Entity
{
    public Guid IntegrationId { get; private set; }
    public DateTime RunDate { get; private set; }
    public bool Success { get; private set; }
    public string? Message { get; private set; }
    public int RowCount { get; private set; }
    public long DurationMs { get; private set; }
    public string? ErrorDetails { get; private set; }

    // Navigation properties
    public Integration? Integration { get; private set; }

    // Private constructor for EF Core
    private IntegrationLog()
    {
    }

    public IntegrationLog(
        Guid integrationId,
        DateTime runDate,
        bool success,
        int rowCount,
        long durationMs,
        string? message = null,
        string? errorDetails = null)
    {
        IntegrationId = integrationId;
        RunDate = runDate;
        Success = success;
        RowCount = rowCount;
        DurationMs = durationMs;
        Message = message;
        ErrorDetails = errorDetails;
    }
}

