using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.IntegrationLogs.Queries.Dtos;

namespace DBIntegrationHub.Application.IntegrationLogs.Queries.GetLogsByIntegrationId;

public record GetLogsByIntegrationIdQuery(Guid IntegrationId) : IQuery<List<IntegrationLogDto>>;

