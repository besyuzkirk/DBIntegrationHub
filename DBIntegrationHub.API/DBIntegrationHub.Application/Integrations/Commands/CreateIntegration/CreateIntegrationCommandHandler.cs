using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Integrations.Commands.CreateIntegration;

public class CreateIntegrationCommandHandler : ICommandHandler<CreateIntegrationCommand, Guid>
{
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIntegrationCommandHandler(
        IIntegrationRepository integrationRepository,
        IConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork)
    {
        _integrationRepository = integrationRepository;
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateIntegrationCommand request, CancellationToken cancellationToken)
    {
        // Kaynak bağlantı kontrolü
        var sourceConnection = await _connectionRepository.GetByIdAsync(request.SourceConnectionId, cancellationToken);
        if (sourceConnection == null)
        {
            return Result.Failure<Guid>("Kaynak bağlantı bulunamadı.");
        }

        // Hedef bağlantı kontrolü
        var targetConnection = await _connectionRepository.GetByIdAsync(request.TargetConnectionId, cancellationToken);
        if (targetConnection == null)
        {
            return Result.Failure<Guid>("Hedef bağlantı bulunamadı.");
        }

        // Integration oluştur
            var integration = Integration.Create(
                request.Name,
                request.SourceConnectionId,
                request.TargetConnectionId,
                request.SourceQuery,
                request.TargetQuery,
                request.GroupName,
                request.ExecutionOrder);

        // Kaydet
        await _integrationRepository.AddAsync(integration, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(integration.Id);
    }
}

