using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DBIntegrationHub.Application.Connections.Commands.PreviewQuery;

public class PreviewQueryCommandHandler : ICommandHandler<PreviewQueryCommand, QueryPreviewResult>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IQueryPreviewService _queryPreviewService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PreviewQueryCommandHandler> _logger;

    public PreviewQueryCommandHandler(
        IConnectionRepository connectionRepository,
        IQueryPreviewService queryPreviewService,
        IEncryptionService encryptionService,
        ILogger<PreviewQueryCommandHandler> logger)
    {
        _connectionRepository = connectionRepository;
        _queryPreviewService = queryPreviewService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<Result<QueryPreviewResult>> Handle(
        PreviewQueryCommand request,
        CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.ConnectionId, cancellationToken);
        if (connection == null)
        {
            return Result.Failure<QueryPreviewResult>($"Bağlantı bulunamadı. ID: {request.ConnectionId}");
        }

        try
        {
            // Connection string'i decrypt et
            var decryptedConnectionString = _encryptionService.Decrypt(connection.ConnectionString);

            var previewData = await _queryPreviewService.GetPreviewDataAsync(
                connection.DatabaseType,
                decryptedConnectionString,
                request.Query,
                100, // max 100 satır
                cancellationToken);

            _logger.LogInformation(
                "Query preview başarılı - Connection: {ConnectionName}, Rows: {Count}",
                connection.Name,
                previewData.RowCount);

            return Result.Success(new QueryPreviewResult(
                previewData.Columns,
                previewData.Rows,
                previewData.RowCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query preview hatası - Connection: {ConnectionId}", request.ConnectionId);
            return Result.Failure<QueryPreviewResult>($"Query çalıştırılırken hata oluştu: {ex.Message}");
        }
    }
}
