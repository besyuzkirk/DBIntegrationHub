using DBIntegrationHub.Application.Abstractions.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace DBIntegrationHub.Infrastructure.Jobs;

public class IntegrationJobScheduler : IIntegrationJobScheduler
{
    private readonly ILogger<IntegrationJobScheduler> _logger;

    public IntegrationJobScheduler(ILogger<IntegrationJobScheduler> logger)
    {
        _logger = logger;
    }

    public string ScheduleJob(Guid scheduledJobId, string cronExpression)
    {
        RecurringJob.AddOrUpdate<IntegrationJobExecutor>(
            scheduledJobId.ToString(),
            executor => executor.ExecuteAsync(scheduledJobId, CancellationToken.None),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });

        _logger.LogInformation("Scheduled job created/updated: {JobId} with cron: {Cron}", scheduledJobId, cronExpression);
        
        return scheduledJobId.ToString();
    }

    public void RemoveJob(string hangfireJobId)
    {
        RecurringJob.RemoveIfExists(hangfireJobId);
        _logger.LogInformation("Scheduled job removed: {JobId}", hangfireJobId);
    }

    public void TriggerJobNow(Guid scheduledJobId)
    {
        RecurringJob.TriggerJob(scheduledJobId.ToString());
        _logger.LogInformation("Scheduled job triggered manually: {JobId}", scheduledJobId);
    }
}

