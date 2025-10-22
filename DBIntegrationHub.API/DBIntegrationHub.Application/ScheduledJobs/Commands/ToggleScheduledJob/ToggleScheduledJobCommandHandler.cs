using DBIntegrationHub.Application.Abstractions.Jobs;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.ToggleScheduledJob;

public class ToggleScheduledJobCommandHandler : ICommandHandler<ToggleScheduledJobCommand>
{
    private readonly IScheduledJobRepository _scheduledJobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationJobScheduler _jobScheduler;

    public ToggleScheduledJobCommandHandler(
        IScheduledJobRepository scheduledJobRepository,
        IUnitOfWork unitOfWork,
        IIntegrationJobScheduler jobScheduler)
    {
        _scheduledJobRepository = scheduledJobRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result> Handle(ToggleScheduledJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _scheduledJobRepository.GetByIdAsync(request.JobId, cancellationToken);
        
        if (job == null)
        {
            return Result.Failure("Zamanlanmış iş bulunamadı.");
        }

        if (job.IsActive)
        {
            job.Deactivate();
            // Hangfire'dan kaldır
            if (!string.IsNullOrEmpty(job.HangfireJobId))
            {
                _jobScheduler.RemoveJob(job.HangfireJobId);
            }
        }
        else
        {
            job.Activate();
            // Hangfire'a tekrar ekle
            var hangfireJobId = _jobScheduler.ScheduleJob(job.Id, job.CronExpression);
            job.SetHangfireJobId(hangfireJobId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

