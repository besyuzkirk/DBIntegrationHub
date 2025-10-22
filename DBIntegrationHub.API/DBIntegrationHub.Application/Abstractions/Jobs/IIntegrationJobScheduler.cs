namespace DBIntegrationHub.Application.Abstractions.Jobs;

public interface IIntegrationJobScheduler
{
    string ScheduleJob(Guid scheduledJobId, string cronExpression);
    void RemoveJob(string hangfireJobId);
    void TriggerJobNow(Guid scheduledJobId);
}

