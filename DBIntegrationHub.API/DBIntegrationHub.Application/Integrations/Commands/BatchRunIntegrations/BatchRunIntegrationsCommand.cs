using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Integrations.Commands.RunIntegration;

namespace DBIntegrationHub.Application.Integrations.Commands.BatchRunIntegrations;

public record BatchRunIntegrationsCommand(List<Guid> IntegrationIds) : ICommand<BatchRunResult>;

public record BatchRunResult(
    bool Success,
    int TotalRowsAffected,
    long TotalDurationMs,
    List<IntegrationRunSummary> Results,
    string? Error = null);

public record IntegrationRunSummary(
    Guid IntegrationId,
    string IntegrationName,
    bool Success,
    int RowsAffected,
    string? Error = null);

