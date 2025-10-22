using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.ScheduledJobs.Commands.ToggleScheduledJob;

public record ToggleScheduledJobCommand(Guid JobId) : ICommand;

