using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Users.Commands.CreateUser;

public record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    IEnumerable<string> RoleNames
) : ICommand<Guid>;

