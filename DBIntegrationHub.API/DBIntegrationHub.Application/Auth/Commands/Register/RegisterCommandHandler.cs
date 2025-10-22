using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Auth.Commands.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcı adı kontrolü
        if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
        {
            return Result.Failure<Guid>("Bu kullanıcı adı zaten kullanılıyor.");
        }

        // Email kontrolü
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<Guid>("Bu email adresi zaten kullanılıyor.");
        }

        // Şifreyi hashle
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Kullanıcı oluştur
        var user = User.Create(request.Username, request.Email, passwordHash);
        await _userRepository.AddAsync(user, cancellationToken);

        // Rolleri ata
        var roleNames = request.RoleNames?.ToList() ?? new List<string> { Role.RoleNames.User };
        
        foreach (var roleName in roleNames)
        {
            var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
            if (role != null)
            {
                user.UserRoles.Add(UserRole.Create(user.Id, role.Id));
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}

