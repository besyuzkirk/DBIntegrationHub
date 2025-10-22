using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Mappings.Queries.Dtos;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Mappings.Queries.GetMappingsByIntegrationId;

public class GetMappingsByIntegrationIdQueryHandler 
    : IQueryHandler<GetMappingsByIntegrationIdQuery, List<MappingDto>>
{
    private readonly IMappingRepository _mappingRepository;

    public GetMappingsByIntegrationIdQueryHandler(IMappingRepository mappingRepository)
    {
        _mappingRepository = mappingRepository;
    }

    public async Task<Result<List<MappingDto>>> Handle(
        GetMappingsByIntegrationIdQuery request,
        CancellationToken cancellationToken)
    {
        var mappings = await _mappingRepository.GetByIntegrationIdAsync(
            request.IntegrationId,
            cancellationToken);

        var dtos = mappings.Select(m => new MappingDto(
            m.Id,
            m.IntegrationId,
            m.SourceColumn,
            m.TargetParameter)).ToList();

        return Result.Success(dtos);
    }
}

