using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Application.Dashboard.Queries;

public class GetIntegrationActivityHandler : IQueryHandler<GetIntegrationActivityQuery, List<IntegrationActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetIntegrationActivityHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<IntegrationActivityDto>>> Handle(
        GetIntegrationActivityQuery request,
        CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-request.Days + 1);
        
        var logs = await _context.IntegrationLogs
            .Where(l => l.RunDate.Date >= startDate)
            .ToListAsync(cancellationToken);

        var activityByDate = logs
            .GroupBy(l => l.RunDate.Date)
            .Select(g => new IntegrationActivityDto(
                Date: g.Key,
                TotalRuns: g.Count(),
                SuccessfulRuns: g.Count(l => l.Success),
                FailedRuns: g.Count(l => !l.Success),
                SuccessRate: g.Any() ? Math.Round((decimal)g.Count(l => l.Success) / g.Count() * 100, 2) : 0
            ))
            .OrderBy(a => a.Date)
            .ToList();

        // Eksik günleri doldur
        var allDates = Enumerable.Range(0, request.Days)
            .Select(i => startDate.AddDays(i))
            .ToList();

        var result = allDates.Select(date =>
        {
            var existing = activityByDate.FirstOrDefault(a => a.Date == date);
            return existing ?? new IntegrationActivityDto(date, 0, 0, 0, 0);
        }).ToList();

        return Result.Success(result);
    }
}

