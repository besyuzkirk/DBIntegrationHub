using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Users.Commands.ToggleUserStatus;

public class ToggleUserStatusCommandHandler : ICommandHandler<ToggleUserStatusCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleUserStatusCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            return Result.Failure("Kullanıcı bulunamadı.");
        }

        // Admin kullanıcısını deaktif etmeyi engelle
        if (user.Username == "admin")
        {
            return Result.Failure("Admin kullanıcısı deaktif edilemez.");
        }

        if (user.IsActive)
            user.Deactivate();
        else
            user.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

