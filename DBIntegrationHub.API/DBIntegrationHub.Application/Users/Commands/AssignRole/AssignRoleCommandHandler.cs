using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Application.Users.Commands.AssignRole;

public class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
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

        // Zaten bu role sahip mi kontrol et
        if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
        {
            return Result.Failure("Kullanıcı zaten bu role sahip.");
        }

        user.UserRoles.Add(UserRole.Create(user.Id, role.Id));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

