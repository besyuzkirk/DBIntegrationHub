using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Integrations.Queries.Dtos;

namespace DBIntegrationHub.Application.Integrations.Queries.GetIntegrationById;

public record GetIntegrationByIdQuery(Guid Id) : IQuery<IntegrationDto>;

