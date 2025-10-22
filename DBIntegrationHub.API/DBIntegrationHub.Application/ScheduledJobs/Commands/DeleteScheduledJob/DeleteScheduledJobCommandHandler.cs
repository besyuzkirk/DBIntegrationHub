using DBIntegrationHub.Application.Abstractions.Jobs;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.DeleteScheduledJob;

public class DeleteScheduledJobCommandHandler : ICommandHandler<DeleteScheduledJobCommand>
{
    private readonly IScheduledJobRepository _scheduledJobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationJobScheduler _jobScheduler;

    public DeleteScheduledJobCommandHandler(
        IScheduledJobRepository scheduledJobRepository,
        IUnitOfWork unitOfWork,
        IIntegrationJobScheduler jobScheduler)
    {
        _scheduledJobRepository = scheduledJobRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result> Handle(DeleteScheduledJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _scheduledJobRepository.GetByIdAsync(request.JobId, cancellationToken);
        
        if (job == null)
        {
            return Result.Failure("Zamanlanmış iş bulunamadı.");
        }

        // Hangfire'dan kaldır
        if (!string.IsNullOrEmpty(job.HangfireJobId))
        {
            _jobScheduler.RemoveJob(job.HangfireJobId);
        }

        await _scheduledJobRepository.DeleteAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

