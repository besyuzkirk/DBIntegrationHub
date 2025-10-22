using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.DeleteScheduledJob;

public record DeleteScheduledJobCommand(Guid JobId) : ICommand;

