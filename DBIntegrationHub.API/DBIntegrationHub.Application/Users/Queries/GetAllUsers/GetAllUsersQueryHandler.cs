using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public GetAllUsersQueryHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        
        var userDtos = new List<UserDto>();
        
        foreach (var user in users)
        {
            var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
            var roleNames = roles.Select(r => r.Name);
            
            userDtos.Add(new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.IsActive,
                user.LastLoginAt,
                user.CreatedAt,
                roleNames
            ));
        }
        
        return Result.Success<IEnumerable<UserDto>>(userDtos);
    }
}

