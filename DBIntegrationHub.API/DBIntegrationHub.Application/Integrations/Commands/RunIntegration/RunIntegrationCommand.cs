using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Integrations.Commands.RunIntegration;

public record RunIntegrationCommand(Guid IntegrationId) : ICommand<RunIntegrationResult>;

public record RunIntegrationResult(
    bool Success,
    int RowsAffected,
    long DurationMs,
    string? Message = null,
    string? Error = null);

