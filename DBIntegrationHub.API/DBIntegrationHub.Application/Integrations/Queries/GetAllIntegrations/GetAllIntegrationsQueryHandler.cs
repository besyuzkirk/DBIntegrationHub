using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Integrations.Queries.Dtos;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Integrations.Queries.GetAllIntegrations;

public class GetAllIntegrationsQueryHandler : IQueryHandler<GetAllIntegrationsQuery, IEnumerable<IntegrationDto>>
{
    private readonly IIntegrationRepository _integrationRepository;

    public GetAllIntegrationsQueryHandler(IIntegrationRepository integrationRepository)
    {
        _integrationRepository = integrationRepository;
    }

    public async Task<Result<IEnumerable<IntegrationDto>>> Handle(
        GetAllIntegrationsQuery request,
        CancellationToken cancellationToken)
    {
        var integrations = await _integrationRepository.GetAllWithConnectionsAsync(cancellationToken);

        var dtos = integrations.Select(i => new IntegrationDto(
            i.Id,
            i.Name,
            i.SourceConnectionId,
            i.SourceConnection?.Name ?? "Unknown",
            i.TargetConnectionId,
            i.TargetConnection?.Name ?? "Unknown",
            i.SourceQuery,
            i.TargetQuery,
            i.GroupName,
            i.ExecutionOrder,
            i.CreatedAt));

        return Result.Success(dtos);
    }
}

