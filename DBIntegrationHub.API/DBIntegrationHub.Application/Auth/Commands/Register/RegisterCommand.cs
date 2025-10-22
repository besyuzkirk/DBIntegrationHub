using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    IEnumerable<string>? RoleNames = null
) : ICommand<Guid>;

