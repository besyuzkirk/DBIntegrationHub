using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    string Username,
    string CurrentPassword,
    string NewPassword
) : ICommand;

