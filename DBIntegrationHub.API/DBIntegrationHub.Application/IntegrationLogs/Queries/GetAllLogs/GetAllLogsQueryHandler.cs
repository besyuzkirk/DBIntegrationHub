using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.IntegrationLogs.Queries.Dtos;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.IntegrationLogs.Queries.GetAllLogs;

public class GetAllLogsQueryHandler : IQueryHandler<GetAllLogsQuery, List<IntegrationLogDto>>
{
    private readonly IIntegrationLogRepository _logRepository;

    public GetAllLogsQueryHandler(IIntegrationLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<List<IntegrationLogDto>>> Handle(
        GetAllLogsQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _logRepository.GetRecentLogsAsync(request.Limit, cancellationToken);

        var dtos = logs.Select(l => new IntegrationLogDto(
            l.Id,
            l.IntegrationId,
            l.RunDate,
            l.Success,
            l.Message,
            l.RowCount,
            l.DurationMs,
            l.ErrorDetails)).ToList();

        return Result.Success(dtos);
    }
}

