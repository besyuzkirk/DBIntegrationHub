using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Users.Queries.GetAllUsers;

public record GetAllUsersQuery : IQuery<IEnumerable<UserDto>>;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    IEnumerable<string> Roles
);

