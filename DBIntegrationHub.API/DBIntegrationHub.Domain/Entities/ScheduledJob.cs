using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Domain.Entities;

public class ScheduledJob : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CronExpression { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    
    // Integration veya Group ID
    public Guid? IntegrationId { get; private set; }
    public Guid? GroupId { get; private set; }
    
    // İstatistikler
    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public int TotalRuns { get; private set; }
    public int SuccessfulRuns { get; private set; }
    public int FailedRuns { get; private set; }
    
    // Hangfire Job ID
    public string? HangfireJobId { get; private set; }

    // Navigation Properties
    public Integration? Integration { get; private set; }

    private ScheduledJob() { } // EF Core için

    private ScheduledJob(
        string name,
        string description,
        string cronExpression,
        Guid? integrationId,
        Guid? groupId)
    {
        Name = name;
        Description = description;
        CronExpression = cronExpression;
        IntegrationId = integrationId;
        GroupId = groupId;
        IsActive = true;
        TotalRuns = 0;
        SuccessfulRuns = 0;
        FailedRuns = 0;
    }

    public static ScheduledJob CreateForIntegration(
        string name,
        string description,
        string cronExpression,
        Guid integrationId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("İş adı boş olamaz", nameof(name));

        if (string.IsNullOrWhiteSpace(cronExpression))
            throw new ArgumentException("Cron ifadesi boş olamaz", nameof(cronExpression));

        return new ScheduledJob(name, description, cronExpression, integrationId, null);
    }

    public static ScheduledJob CreateForGroup(
        string name,
        string description,
        string cronExpression,
        Guid groupId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("İş adı boş olamaz", nameof(name));

        if (string.IsNullOrWhiteSpace(cronExpression))
            throw new ArgumentException("Cron ifadesi boş olamaz", nameof(cronExpression));

        return new ScheduledJob(name, description, cronExpression, null, groupId);
    }

    public void Update(string name, string description, string cronExpression)
    {
        Name = name;
        Description = description;
        CronExpression = cronExpression;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetHangfireJobId(string hangfireJobId)
    {
        HangfireJobId = hangfireJobId;
    }

    public void RecordRun(bool success, DateTime? nextRun = null)
    {
        LastRunAt = DateTime.UtcNow;
        NextRunAt = nextRun;
        TotalRuns++;
        
        if (success)
            SuccessfulRuns++;
        else
            FailedRuns++;
        
        UpdatedAt = DateTime.UtcNow;
    }
}

