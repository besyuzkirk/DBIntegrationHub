using DBIntegrationHub.Application.Abstractions.Jobs;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.CreateScheduledJob;

public class CreateScheduledJobCommandHandler : ICommandHandler<CreateScheduledJobCommand, Guid>
{
    private readonly IScheduledJobRepository _scheduledJobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIntegrationJobScheduler _jobScheduler;

    public CreateScheduledJobCommandHandler(
        IScheduledJobRepository scheduledJobRepository,
        IUnitOfWork unitOfWork,
        IIntegrationJobScheduler jobScheduler)
    {
        _scheduledJobRepository = scheduledJobRepository;
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
    }

    public async Task<Result<Guid>> Handle(CreateScheduledJobCommand request, CancellationToken cancellationToken)
    {
        if (!request.IntegrationId.HasValue && !request.GroupId.HasValue)
        {
            return Result.Failure<Guid>("Integration veya Group seçilmelidir.");
        }

        if (request.IntegrationId.HasValue && request.GroupId.HasValue)
        {
            return Result.Failure<Guid>("Sadece Integration veya Group seçilebilir, ikisi birden seçilemez.");
        }

        ScheduledJob job;
        
        if (request.IntegrationId.HasValue)
        {
            job = ScheduledJob.CreateForIntegration(
                request.Name,
                request.Description,
                request.CronExpression,
                request.IntegrationId.Value);
        }
        else
        {
            job = ScheduledJob.CreateForGroup(
                request.Name,
                request.Description,
                request.CronExpression,
                request.GroupId!.Value);
        }

        await _scheduledJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Hangfire'a kaydet
        var hangfireJobId = _jobScheduler.ScheduleJob(job.Id, request.CronExpression);
        job.SetHangfireJobId(hangfireJobId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(job.Id);
    }
}

