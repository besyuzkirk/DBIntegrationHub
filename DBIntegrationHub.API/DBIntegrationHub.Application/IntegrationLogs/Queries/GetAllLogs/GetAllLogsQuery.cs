using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.IntegrationLogs.Queries.Dtos;

namespace DBIntegrationHub.Application.IntegrationLogs.Queries.GetAllLogs;

public record GetAllLogsQuery(int Limit = 100) : IQuery<List<IntegrationLogDto>>;

