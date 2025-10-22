using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Integrations.Commands.CreateIntegration;

public record CreateIntegrationCommand(
    string Name,
    Guid SourceConnectionId,
    Guid TargetConnectionId,
    string SourceQuery,
    string TargetQuery,
    string? GroupName = null,
    int ExecutionOrder = 0
) : ICommand<Guid>;

