using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Users.Queries.GetAllRoles;

public record GetAllRolesQuery : IQuery<IEnumerable<RoleDto>>;

public record RoleDto(
    Guid Id,
    string Name,
    string Description
);

