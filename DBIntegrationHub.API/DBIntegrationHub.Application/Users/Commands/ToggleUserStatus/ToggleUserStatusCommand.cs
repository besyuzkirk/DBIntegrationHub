using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Users.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(Guid UserId) : ICommand;

