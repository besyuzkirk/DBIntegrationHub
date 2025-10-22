using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Mappings.Queries.Dtos;

namespace DBIntegrationHub.Application.Mappings.Queries.GetMappingsByIntegrationId;

public record GetMappingsByIntegrationIdQuery(Guid IntegrationId) : IQuery<List<MappingDto>>;

