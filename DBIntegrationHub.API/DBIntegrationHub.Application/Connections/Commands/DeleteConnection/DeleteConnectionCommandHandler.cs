using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Connections.Commands.DeleteConnection;

public class DeleteConnectionCommandHandler : ICommandHandler<DeleteConnectionCommand>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteConnectionCommandHandler(
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteConnectionCommand request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (connection == null)
        {
            return Result.Failure($"Connection bulunamadı. Id: {request.Id}");
        }

        await _connectionRepository.DeleteAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

