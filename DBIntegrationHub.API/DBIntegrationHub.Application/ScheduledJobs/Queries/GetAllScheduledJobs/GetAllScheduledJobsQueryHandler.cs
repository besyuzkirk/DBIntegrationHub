using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.ScheduledJobs.Queries.GetAllScheduledJobs;

public class GetAllScheduledJobsQueryHandler : IQueryHandler<GetAllScheduledJobsQuery, IEnumerable<ScheduledJobDto>>
{
    private readonly IScheduledJobRepository _scheduledJobRepository;

    public GetAllScheduledJobsQueryHandler(IScheduledJobRepository scheduledJobRepository)
    {
        _scheduledJobRepository = scheduledJobRepository;
    }

    public async Task<Result<IEnumerable<ScheduledJobDto>>> Handle(GetAllScheduledJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _scheduledJobRepository.GetAllAsync(cancellationToken);
        
        var jobDtos = jobs.Select(j => new ScheduledJobDto(
            j.Id,
            j.Name,
            j.Description,
            j.CronExpression,
            j.IsActive,
            j.IntegrationId,
            j.Integration?.Name,
            j.GroupId,
            j.LastRunAt,
            j.NextRunAt,
            j.TotalRuns,
            j.SuccessfulRuns,
            j.FailedRuns,
            j.CreatedAt
        ));
        
        return Result.Success<IEnumerable<ScheduledJobDto>>(jobDtos);
    }
}

