using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Users.Queries.GetAllRoles;

public class GetAllRolesQueryHandler : IQueryHandler<GetAllRolesQuery, IEnumerable<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IEnumerable<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        
        var roleDtos = roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description
        ));
        
        return Result.Success<IEnumerable<RoleDto>>(roleDtos);
    }
}

