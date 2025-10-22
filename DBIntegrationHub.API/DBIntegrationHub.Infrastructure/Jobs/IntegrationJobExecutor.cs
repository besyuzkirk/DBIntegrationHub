using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace DBIntegrationHub.Infrastructure.Jobs;

public class IntegrationJobExecutor
{
    private readonly IScheduledJobRepository _scheduledJobRepository;
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IIntegrationRunner _integrationRunner;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationJobExecutor> _logger;

    public IntegrationJobExecutor(
        IScheduledJobRepository scheduledJobRepository,
        IIntegrationRepository integrationRepository,
        IIntegrationRunner integrationRunner,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationJobExecutor> logger)
    {
        _scheduledJobRepository = scheduledJobRepository;
        _integrationRepository = integrationRepository;
        _integrationRunner = integrationRunner;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid scheduledJobId, CancellationToken cancellationToken)
    {
        var job = await _scheduledJobRepository.GetByIdAsync(scheduledJobId, cancellationToken);
        
        if (job == null)
        {
            _logger.LogWarning("Scheduled job not found: {JobId}", scheduledJobId);
            return;
        }

        if (!job.IsActive)
        {
            _logger.LogInformation("Scheduled job is inactive, skipping: {JobId}", scheduledJobId);
            return;
        }

        _logger.LogInformation("Executing scheduled job: {JobName} ({JobId})", job.Name, scheduledJobId);

        try
        {
            if (job.IntegrationId.HasValue)
            {
                // Tek integration çalıştır
                await ExecuteIntegrationAsync(job.IntegrationId.Value, cancellationToken);
                job.RecordRun(true, CalculateNextRun(job.CronExpression));
            }
            else if (job.GroupId.HasValue)
            {
                // Group içindeki tüm integration'ları çalıştır
                await ExecuteGroupIntegrationsAsync(job.GroupId.Value, cancellationToken);
                job.RecordRun(true, CalculateNextRun(job.CronExpression));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Scheduled job completed successfully: {JobName}", job.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scheduled job: {JobName}", job.Name);
            job.RecordRun(false, CalculateNextRun(job.CronExpression));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ExecuteIntegrationAsync(Guid integrationId, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdWithConnectionsAsync(integrationId, cancellationToken);
        
        if (integration == null)
        {
            _logger.LogWarning("Integration not found: {IntegrationId}", integrationId);
            return;
        }

        if (integration.SourceConnection == null || integration.TargetConnection == null)
        {
            _logger.LogWarning("Integration connections not loaded: {IntegrationName}", integration.Name);
            return;
        }

        await _integrationRunner.RunAsync(
            integration.SourceConnection.DatabaseType,
            integration.SourceConnection.ConnectionString,
            integration.SourceQuery,
            integration.TargetConnection.DatabaseType,
            integration.TargetConnection.ConnectionString,
            integration.TargetQuery,
            null,
            cancellationToken);
    }

    private async Task ExecuteGroupIntegrationsAsync(Guid groupId, CancellationToken cancellationToken)
    {
        // Grup adına göre tüm integration'ları al
        var allIntegrations = await _integrationRepository.GetAllWithConnectionsAsync(cancellationToken);
        var groupIntegrations = allIntegrations
            .Where(i => i.GroupName == groupId.ToString())
            .OrderBy(i => i.ExecutionOrder)
            .ToList();
        
        foreach (var integration in groupIntegrations)
        {
            if (integration.SourceConnection == null || integration.TargetConnection == null)
            {
                _logger.LogWarning("Integration connections not loaded: {IntegrationName}", integration.Name);
                continue;
            }

            await _integrationRunner.RunAsync(
                integration.SourceConnection.DatabaseType,
                integration.SourceConnection.ConnectionString,
                integration.SourceQuery,
                integration.TargetConnection.DatabaseType,
                integration.TargetConnection.ConnectionString,
                integration.TargetQuery,
                null,
                cancellationToken);
        }
    }

    private DateTime? CalculateNextRun(string cronExpression)
    {
        try
        {
            // Basit next run hesaplama - production'da Cronos kütüphanesi kullanılabilir
            return DateTime.UtcNow.AddHours(1);
        }
        catch
        {
            return null;
        }
    }
}

