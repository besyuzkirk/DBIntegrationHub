using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Application.Dashboard.Queries;

public class GetRecentIntegrationLogsHandler : IQueryHandler<GetRecentIntegrationLogsQuery, List<RecentIntegrationLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentIntegrationLogsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RecentIntegrationLogDto>>> Handle(
        GetRecentIntegrationLogsQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _context.IntegrationLogs
            .Include(l => l.Integration)
            .OrderByDescending(l => l.RunDate)
            .Take(request.Count)
            .Select(l => new RecentIntegrationLogDto(
                l.Id,
                l.IntegrationId,
                l.Integration!.Name,
                l.RunDate,
                l.Success,
                l.RowCount,
                l.DurationMs,
                l.Message,
                l.ErrorDetails
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(logs);
    }
}

