using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Integrations.Commands.DeleteIntegration;

public class DeleteIntegrationCommandHandler : ICommandHandler<DeleteIntegrationCommand>
{
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteIntegrationCommandHandler(
        IIntegrationRepository integrationRepository,
        IUnitOfWork unitOfWork)
    {
        _integrationRepository = integrationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteIntegrationCommand request, CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (integration == null)
        {
            return Result.Failure($"Integration bulunamadı. Id: {request.Id}");
        }

        await _integrationRepository.DeleteAsync(integration, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

