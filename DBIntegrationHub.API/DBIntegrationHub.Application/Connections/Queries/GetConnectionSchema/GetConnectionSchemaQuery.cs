using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;

public record GetConnectionSchemaQuery(Guid ConnectionId) : IQuery<ConnectionSchemaResponse>;
