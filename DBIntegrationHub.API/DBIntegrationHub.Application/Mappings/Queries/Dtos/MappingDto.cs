namespace DBIntegrationHub.Application.Mappings.Queries.Dtos;

public record MappingDto(
    Guid Id,
    Guid IntegrationId,
    string SourceColumn,
    string TargetParameter);

