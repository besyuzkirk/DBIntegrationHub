using DBIntegrationHub.Application.Abstractions.Jobs;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.TriggerScheduledJob;

public class TriggerScheduledJobCommandHandler : ICommandHandler<TriggerScheduledJobCommand>
{
    private readonly IScheduledJobRepository _scheduledJobRepository;
    private readonly IIntegrationJobScheduler _jobScheduler;

    public TriggerScheduledJobCommandHandler(
        IScheduledJobRepository scheduledJobRepository,
        IIntegrationJobScheduler jobScheduler)
    {
        _scheduledJobRepository = scheduledJobRepository;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result> Handle(TriggerScheduledJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _scheduledJobRepository.GetByIdAsync(request.JobId, cancellationToken);
        
        if (job == null)
        {
            return Result.Failure("Zamanlanmış iş bulunamadı.");
        }

        // Şimdi çalıştır
        _jobScheduler.TriggerJobNow(job.Id);

        return Result.Success();
    }
}

