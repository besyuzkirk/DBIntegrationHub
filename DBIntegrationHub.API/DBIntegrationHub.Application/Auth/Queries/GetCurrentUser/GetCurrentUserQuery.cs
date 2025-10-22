using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IQuery<UserDto>;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DateTime? LastLoginAt,
    IEnumerable<string> Roles
);

