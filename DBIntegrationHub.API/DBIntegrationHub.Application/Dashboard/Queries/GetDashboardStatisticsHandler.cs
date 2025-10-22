using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Application.Dashboard.Queries;

public class GetDashboardStatisticsHandler : IQueryHandler<GetDashboardStatisticsQuery, DashboardStatisticsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardStatisticsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<DashboardStatisticsResponse>> Handle(
        GetDashboardStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        
        // Connection istatistikleri
        var totalConnections = await _context.Connections.CountAsync(cancellationToken);
        var activeConnections = await _context.Connections.CountAsync(c => c.IsActive, cancellationToken);
        var inactiveConnections = totalConnections - activeConnections;

        // Integration ve Mapping sayıları
        var totalIntegrations = await _context.Integrations.CountAsync(cancellationToken);
        var totalMappings = await _context.Mappings.CountAsync(cancellationToken);

        // Log istatistikleri
        var totalLogs = await _context.IntegrationLogs.CountAsync(cancellationToken);
        var todayLogs = await _context.IntegrationLogs
            .CountAsync(l => l.RunDate.Date == today, cancellationToken);
        
        var successfulLogsToday = await _context.IntegrationLogs
            .CountAsync(l => l.RunDate.Date == today && l.Success, cancellationToken);
        
        var failedLogsToday = todayLogs - successfulLogsToday;

        // Başarı oranları
        var successRateToday = todayLogs > 0 
            ? Math.Round((decimal)successfulLogsToday / todayLogs * 100, 2)
            : 0;

        var totalSuccessfulLogs = await _context.IntegrationLogs
            .CountAsync(l => l.Success, cancellationToken);
        
        var overallSuccessRate = totalLogs > 0
            ? Math.Round((decimal)totalSuccessfulLogs / totalLogs * 100, 2)
            : 0;

        var response = new DashboardStatisticsResponse(
            TotalConnections: totalConnections,
            ActiveConnections: activeConnections,
            InactiveConnections: inactiveConnections,
            TotalIntegrations: totalIntegrations,
            TotalMappings: totalMappings,
            TotalLogs: totalLogs,
            TodayLogs: todayLogs,
            SuccessfulLogsToday: successfulLogsToday,
            FailedLogsToday: failedLogsToday,
            SuccessRateToday: successRateToday,
            OverallSuccessRate: overallSuccessRate
        );

        return Result.Success(response);
    }
}

