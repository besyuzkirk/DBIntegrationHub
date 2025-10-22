using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Connections.Queries.Dtos;

namespace DBIntegrationHub.Application.Connections.Queries.GetAllConnections;

public record GetAllConnectionsQuery : IQuery<IEnumerable<ConnectionDto>>;

