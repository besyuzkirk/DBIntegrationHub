namespace DBIntegrationHub.Application.Integrations.Queries.Dtos;

public record IntegrationDto(
    Guid Id,
    string Name,
    Guid SourceConnectionId,
    string SourceConnectionName,
    Guid TargetConnectionId,
    string TargetConnectionName,
    string SourceQuery,
    string TargetQuery,
    string? GroupName,
    int ExecutionOrder,
    DateTime CreatedAt);

