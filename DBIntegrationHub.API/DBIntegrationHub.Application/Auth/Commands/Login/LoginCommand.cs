using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Auth.Commands.Login;

public record LoginCommand(
    string Username,
    string Password
) : ICommand<LoginResponse>;

public record LoginResponse(
    string Token,
    string Username,
    string Email,
    IEnumerable<string> Roles
);

