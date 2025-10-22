using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId) : ICommand;

