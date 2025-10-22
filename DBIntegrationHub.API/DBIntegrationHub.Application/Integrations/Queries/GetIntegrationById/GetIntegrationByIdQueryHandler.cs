using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Integrations.Queries.Dtos;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Integrations.Queries.GetIntegrationById;

public class GetIntegrationByIdQueryHandler : IQueryHandler<GetIntegrationByIdQuery, IntegrationDto>
{
    private readonly IIntegrationRepository _integrationRepository;

    public GetIntegrationByIdQueryHandler(IIntegrationRepository integrationRepository)
    {
        _integrationRepository = integrationRepository;
    }

    public async Task<Result<IntegrationDto>> Handle(
        GetIntegrationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var integration = await _integrationRepository.GetByIdWithConnectionsAsync(request.Id, cancellationToken);

        if (integration == null)
        {
            return Result.Failure<IntegrationDto>($"Integration bulunamadı. Id: {request.Id}");
        }

        var dto = new IntegrationDto(
            integration.Id,
            integration.Name,
            integration.SourceConnectionId,
            integration.SourceConnection?.Name ?? "Unknown",
            integration.TargetConnectionId,
            integration.TargetConnection?.Name ?? "Unknown",
            integration.SourceQuery,
            integration.TargetQuery,
            integration.GroupName,
            integration.ExecutionOrder,
            integration.CreatedAt);

        return Result.Success(dto);
    }
}

