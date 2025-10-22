namespace DBIntegrationHub.Application.Connections.Queries.Dtos;

public record ConnectionDto(
    Guid Id,
    string Name,
    string ConnectionString,
    string DatabaseType,
    bool IsActive,
    DateTime CreatedAt);

