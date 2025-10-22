using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return Result.Failure<UserDto>("Kullanıcı oturum açmamış.");
        }

        var user = await _userRepository.GetByIdWithRolesAsync(_currentUserService.UserId.Value, cancellationToken);
        
        if (user == null)
        {
            return Result.Failure<UserDto>("Kullanıcı bulunamadı.");
        }

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        var roleNames = roles.Select(r => r.Name);

        var userDto = new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.IsActive,
            user.LastLoginAt,
            roleNames
        );

        return Result.Success(userDto);
    }
}

