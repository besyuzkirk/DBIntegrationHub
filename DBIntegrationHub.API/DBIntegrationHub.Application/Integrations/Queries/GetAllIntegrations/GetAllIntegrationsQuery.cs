using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Integrations.Queries.Dtos;

namespace DBIntegrationHub.Application.Integrations.Queries.GetAllIntegrations;

public record GetAllIntegrationsQuery : IQuery<IEnumerable<IntegrationDto>>;

