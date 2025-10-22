using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Integrations.Queries.GetIntegrationColumns;

public record GetIntegrationColumnsQuery(Guid IntegrationId) : IQuery<IntegrationColumnsDto>;

public record IntegrationColumnsDto(
    List<string> SourceColumns,
    List<string> TargetParameters);

