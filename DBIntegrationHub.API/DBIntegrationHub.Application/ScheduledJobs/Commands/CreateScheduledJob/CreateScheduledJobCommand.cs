using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.CreateScheduledJob;

public record CreateScheduledJobCommand(
    string Name,
    string Description,
    string CronExpression,
    Guid? IntegrationId,
    Guid? GroupId
) : ICommand<Guid>;

