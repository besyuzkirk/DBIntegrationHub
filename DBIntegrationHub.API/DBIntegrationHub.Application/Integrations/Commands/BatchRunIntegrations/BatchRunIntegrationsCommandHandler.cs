using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DBIntegrationHub.Application.Integrations.Commands.BatchRunIntegrations;

public class BatchRunIntegrationsCommandHandler 
    : ICommandHandler<BatchRunIntegrationsCommand, BatchRunResult>
{
    private readonly IIntegrationRepository _integrationRepository;
    private readonly IConnectionRepository _connectionRepository;
    private readonly IMappingRepository _mappingRepository;
    private readonly IIntegrationLogRepository _logRepository;
    private readonly IIntegrationRunner _integrationRunner;
    private readonly IEncryptionService _encryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BatchRunIntegrationsCommandHandler> _logger;

    public BatchRunIntegrationsCommandHandler(
        IIntegrationRepository integrationRepository,
        IConnectionRepository connectionRepository,
        IMappingRepository mappingRepository,
        IIntegrationLogRepository logRepository,
        IIntegrationRunner integrationRunner,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork,
        ILogger<BatchRunIntegrationsCommandHandler> logger)
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

    public async Task<Result<BatchRunResult>> Handle(
        BatchRunIntegrationsCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<IntegrationRunSummary>();
        int totalRows = 0;

        try
        {
            // Tüm integration'ları al ve ExecutionOrder'a göre sırala
            var integrations = new List<Domain.Entities.Integration>();
            foreach (var id in request.IntegrationIds)
            {
                var integration = await _integrationRepository.GetByIdWithConnectionsAsync(id, cancellationToken);
                if (integration != null) integrations.Add(integration);
            }

            integrations = integrations.OrderBy(i => i.ExecutionOrder).ToList();

            // Her birini sırayla çalıştır
            foreach (var integration in integrations)
            {
                try
                {
                    var sourceConn = await _connectionRepository.GetByIdAsync(integration.SourceConnectionId, cancellationToken);
                    var targetConn = await _connectionRepository.GetByIdAsync(integration.TargetConnectionId, cancellationToken);
                    
                    if (sourceConn == null || targetConn == null)
                    {
                        results.Add(new IntegrationRunSummary(integration.Id, integration.Name, false, 0, "Bağlantılar bulunamadı"));
                        throw new Exception($"Integration {integration.Name} için bağlantılar bulunamadı - ROLLBACK");
                    }

                    var mappings = await _mappingRepository.GetByIntegrationIdAsync(integration.Id, cancellationToken);
                    if (!mappings.Any())
                    {
                        results.Add(new IntegrationRunSummary(integration.Id, integration.Name, false, 0, "Mapping tanımlanmamış"));
                        throw new Exception($"Integration {integration.Name} için mapping tanımlanmamış - ROLLBACK");
                    }

                    var columnMappings = mappings.ToDictionary(m => m.TargetParameter, m => m.SourceColumn);

                    // Connection string'leri decrypt et
                    var decryptedSourceConnectionString = _encryptionService.Decrypt(sourceConn.ConnectionString);
                    var decryptedTargetConnectionString = _encryptionService.Decrypt(targetConn.ConnectionString);

                    var result = await _integrationRunner.RunAsync(
                        sourceConn.DatabaseType, decryptedSourceConnectionString, integration.SourceQuery,
                        targetConn.DatabaseType, decryptedTargetConnectionString, integration.TargetQuery,
                        columnMappings, cancellationToken);

                    if (!result.Success)
                    {
                        results.Add(new IntegrationRunSummary(integration.Id, integration.Name, false, 0, result.Error));
                        throw new Exception($"Integration {integration.Name} başarısız oldu - ROLLBACK");
                    }

                    totalRows += result.RowsAffected;
                    results.Add(new IntegrationRunSummary(integration.Id, integration.Name, true, result.RowsAffected));

                    // Log kaydet
                    await _logRepository.AddAsync(new IntegrationLog(integration.Id, DateTime.UtcNow, true,
                        result.RowsAffected, result.DurationMs, result.Message), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Batch run başarısız - Integration: {Name}", integration.Name);
                    throw; // Rollback için
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("Batch run başarılı - {Count} integration, {Rows} satır, {Duration}ms",
                integrations.Count, totalRows, stopwatch.ElapsedMilliseconds);

            return Result.Success(new BatchRunResult(true, totalRows, stopwatch.ElapsedMilliseconds, results));
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Batch run başarısız - ROLLBACK");

            return Result.Success(new BatchRunResult(false, 0, stopwatch.ElapsedMilliseconds, results,
                "Batch run başarısız oldu. Tüm işlemler geri alındı: " + ex.Message));
        }
    }
}

