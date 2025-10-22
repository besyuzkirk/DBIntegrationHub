using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Users.Commands.AssignRole;

public record AssignRoleCommand(
    Guid UserId,
    string RoleName
) : ICommand;

