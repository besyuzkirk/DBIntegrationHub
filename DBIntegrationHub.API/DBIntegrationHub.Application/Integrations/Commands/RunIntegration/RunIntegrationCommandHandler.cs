using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DBIntegrationHub.Application.Integrations.Commands.RunIntegration;

public class RunIntegrationCommandHandler 
    : ICommandHandler<RunIntegrationCommand, RunIntegrationResult>
{
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IMappingRepository _mappingRepository;
    private readonly IIntegrationLogRepository _logRepository;
    private readonly IIntegrationRunner _integrationRunner;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RunIntegrationCommandHandler> _logger;

    public RunIntegrationCommandHandler(
        IIntegrationRepository integrationRepository,
        IConnectionRepository connectionRepository,
        IMappingRepository mappingRepository,
        IIntegrationLogRepository logRepository,
        IIntegrationRunner integrationRunner,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork,
        ILogger<RunIntegrationCommandHandler> logger)
    {
        _integrationRepository = integrationRepository;
        _connectionRepository = connectionRepository;
        _mappingRepository = mappingRepository;
        _logRepository = logRepository;
        _integrationRunner = integrationRunner;
        _encryptionService = encryptionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<RunIntegrationResult>> Handle(
        RunIntegrationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Integration'ı al
            var integration = await _integrationRepository.GetByIdWithConnectionsAsync(
                request.IntegrationId,
                cancellationToken);

            if (integration == null)
            {
                return Result.Failure<RunIntegrationResult>(
                    $"Integration bulunamadı. Id: {request.IntegrationId}");
            }

            // Bağlantıları al
            var sourceConnection = await _connectionRepository.GetByIdAsync(
                integration.SourceConnectionId,
                cancellationToken);

            var targetConnection = await _connectionRepository.GetByIdAsync(
                integration.TargetConnectionId,
                cancellationToken);

            if (sourceConnection == null || targetConnection == null)
            {
                return Result.Failure<RunIntegrationResult>("Bağlantılar bulunamadı.");
            }

            // Mapping'leri al (ZORUNLU)
            var mappings = await _mappingRepository.GetByIntegrationIdAsync(
                request.IntegrationId,
                cancellationToken);

            if (!mappings.Any())
            {
                return Result.Failure<RunIntegrationResult>(
                    "Bu integration için mapping tanımlanmamış. Lütfen önce mapping yapın.");
            }

            // TargetParameter -> SourceColumn mapping
            var columnMappings = mappings.ToDictionary(
                m => m.TargetParameter,
                m => m.SourceColumn);

            // Connection string'leri decrypt et
            var decryptedSourceConnectionString = _encryptionService.Decrypt(sourceConnection.ConnectionString);
            var decryptedTargetConnectionString = _encryptionService.Decrypt(targetConnection.ConnectionString);

            // Integration'ı çalıştır
            var result = await _integrationRunner.RunAsync(
                sourceConnection.DatabaseType,
                decryptedSourceConnectionString,
                integration.SourceQuery,
                targetConnection.DatabaseType,
                decryptedTargetConnectionString,
                integration.TargetQuery,
                columnMappings,
                cancellationToken);

            // Log kaydet
            var log = new IntegrationLog(
                integrationId: request.IntegrationId,
                runDate: DateTime.UtcNow,
                success: result.Success,
                rowCount: result.RowsAffected,
                durationMs: result.DurationMs,
                message: result.Message,
                errorDetails: result.Error);

            await _logRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Integration çalıştırıldı - Id: {IntegrationId}, Başarı: {Success}, Satır: {Rows}, Süre: {Duration}ms",
                request.IntegrationId,
                result.Success,
                result.RowsAffected,
                result.DurationMs);

            return Result.Success(new RunIntegrationResult(
                result.Success,
                result.RowsAffected,
                result.DurationMs,
                result.Message,
                result.Error));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Integration çalıştırılırken hata - Id: {IntegrationId}",
                request.IntegrationId);

            // Hata durumunda da log kaydet
            try
            {
                var errorLog = new IntegrationLog(
                    integrationId: request.IntegrationId,
                    runDate: DateTime.UtcNow,
                    success: false,
                    rowCount: 0,
                    durationMs: 0,
                    message: null,
                    errorDetails: ex.Message);

                await _logRepository.AddAsync(errorLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Log kaydedilemedi");
            }

            return Result.Failure<RunIntegrationResult>(
                $"Integration çalıştırılırken hata oluştu: {ex.Message}");
        }
    }
}
