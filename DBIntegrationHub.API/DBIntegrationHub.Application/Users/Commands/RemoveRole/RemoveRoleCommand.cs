using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Users.Commands.RemoveRole;

public record RemoveRoleCommand(
    Guid UserId,
    string RoleName
) : ICommand;

