using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.TriggerScheduledJob;

public record TriggerScheduledJobCommand(Guid JobId) : ICommand;

