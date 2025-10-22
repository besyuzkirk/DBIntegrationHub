using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Connections.Queries.Dtos;

namespace DBIntegrationHub.Application.Connections.Queries.GetConnectionById;

public record GetConnectionByIdQuery(Guid Id) : IQuery<ConnectionDto>;

