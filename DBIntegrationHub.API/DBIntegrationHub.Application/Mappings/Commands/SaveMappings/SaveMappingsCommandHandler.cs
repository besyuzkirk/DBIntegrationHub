using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Domain.Entities;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DBIntegrationHub.Application.Mappings.Commands.SaveMappings;

public class SaveMappingsCommandHandler : ICommandHandler<SaveMappingsCommand, bool>
{
    private readonly IMappingRepository _mappingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SaveMappingsCommandHandler> _logger;

    public SaveMappingsCommandHandler(
        IMappingRepository mappingRepository,
        IUnitOfWork unitOfWork,
        ILogger<SaveMappingsCommandHandler> logger)
    {
        _mappingRepository = mappingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        SaveMappingsCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Eski mapping'leri sil
            await _mappingRepository.DeleteByIntegrationIdAsync(
                request.IntegrationId,
                cancellationToken);

            // Yeni mapping'leri ekle
            foreach (var item in request.Mappings)
            {
                var mapping = new Mapping(
                    request.IntegrationId,
                    item.SourceColumn,
                    item.TargetParameter);

                await _mappingRepository.AddAsync(mapping, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Mapping'ler kaydedildi - IntegrationId: {IntegrationId}, Sayı: {Count}",
                request.IntegrationId,
                request.Mappings.Count);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Mapping'ler kaydedilirken hata - IntegrationId: {IntegrationId}",
                request.IntegrationId);

            return Result.Failure<bool>($"Mapping'ler kaydedilirken hata oluştu: {ex.Message}");
        }
    }
}

