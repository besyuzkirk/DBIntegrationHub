using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.IntegrationLogs.Queries.Dtos;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.IntegrationLogs.Queries.GetLogsByIntegrationId;

public class GetLogsByIntegrationIdQueryHandler 
    : IQueryHandler<GetLogsByIntegrationIdQuery, List<IntegrationLogDto>>
{
    private readonly IIntegrationLogRepository _logRepository;

    public GetLogsByIntegrationIdQueryHandler(IIntegrationLogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<Result<List<IntegrationLogDto>>> Handle(
        GetLogsByIntegrationIdQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _logRepository.GetByIntegrationIdAsync(
            request.IntegrationId,
            cancellationToken);

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

