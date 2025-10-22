using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Integrations.Queries.GetIntegrationColumns;

public class GetIntegrationColumnsQueryHandler 
    : IQueryHandler<GetIntegrationColumnsQuery, IntegrationColumnsDto>
{
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IQueryPreviewService _queryPreviewService;
    private readonly IEncryptionService _encryptionService;

    public GetIntegrationColumnsQueryHandler(
        IIntegrationRepository integrationRepository,
        IConnectionRepository connectionRepository,
        IQueryPreviewService queryPreviewService,
        IEncryptionService encryptionService)
    {
        _integrationRepository = integrationRepository;
        _connectionRepository = connectionRepository;
        _queryPreviewService = queryPreviewService;
        _encryptionService = encryptionService;
    }

    public async Task<Result<IntegrationColumnsDto>> Handle(
        GetIntegrationColumnsQuery request,
        CancellationToken cancellationToken)
    {
        // Integration'ı al
        var integration = await _integrationRepository.GetByIdWithConnectionsAsync(
            request.IntegrationId, 
            cancellationToken);

        if (integration == null)
        {
            return Result.Failure<IntegrationColumnsDto>(
                $"Integration bulunamadı. Id: {request.IntegrationId}");
        }

        // Source connection'ı al
        var sourceConnection = await _connectionRepository.GetByIdAsync(
            integration.SourceConnectionId, 
            cancellationToken);

        if (sourceConnection == null)
        {
            return Result.Failure<IntegrationColumnsDto>("Kaynak bağlantı bulunamadı.");
        }

        try
        {
            // Connection string'i decrypt et
            var decryptedConnectionString = _encryptionService.Decrypt(sourceConnection.ConnectionString);

            // Source query'den kolonları al (SchemaOnly ile)
            var sourceColumns = await _queryPreviewService.GetColumnNamesAsync(
                sourceConnection.DatabaseType,
                decryptedConnectionString,
                integration.SourceQuery,
                cancellationToken);

            // Target query'den parametreleri çıkar (Regex ile)
            var targetParameters = _queryPreviewService.ExtractParametersFromQuery(
                integration.TargetQuery);

            var dto = new IntegrationColumnsDto(sourceColumns, targetParameters);

            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            return Result.Failure<IntegrationColumnsDto>(
                $"Kolon bilgileri alınamadı: {ex.Message}");
        }
    }
}

