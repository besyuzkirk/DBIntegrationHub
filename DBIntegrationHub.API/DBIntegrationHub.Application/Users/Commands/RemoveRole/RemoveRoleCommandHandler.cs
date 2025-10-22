using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Users.Commands.RemoveRole;

public class RemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            return Result.Failure("Kullanıcı bulunamadı.");
        }

        var role = await _roleRepository.GetByNameAsync(request.RoleName, cancellationToken);
        
        if (role == null)
        {
            return Result.Failure("Rol bulunamadı.");
        }

        // Admin kullanıcısından Admin rolünü çıkarmayı engelle
        if (user.Username == "admin" && request.RoleName == "Admin")
        {
            return Result.Failure("Admin kullanıcısından Admin rolü çıkarılamaz.");
        }

        var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
        
        if (userRole == null)
        {
            return Result.Failure("Kullanıcının bu rolü yok.");
        }

        user.UserRoles.Remove(userRole);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

