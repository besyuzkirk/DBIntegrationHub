using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Auth.Commands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcıyı bul
        var user = await _userRepository.GetByUsernameWithRolesAsync(request.Username, cancellationToken);
        
        if (user == null)
        {
            return Result.Failure<LoginResponse>("Kullanıcı adı veya şifre hatalı.");
        }

        // Aktif mi kontrol et
        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>("Bu kullanıcı hesabı devre dışı bırakılmış.");
        }

        // Şifre kontrolü
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>("Kullanıcı adı veya şifre hatalı.");
        }

        // Rolleri al
        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        var roleNames = roles.Select(r => r.Name).ToList();

        // Son giriş zamanını güncelle
        user.UpdateLastLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // JWT token oluştur
        var token = _jwtService.GenerateToken(user, roleNames);

        return Result.Success(new LoginResponse(
            token,
            user.Username,
            user.Email,
            roleNames
        ));
    }
}

