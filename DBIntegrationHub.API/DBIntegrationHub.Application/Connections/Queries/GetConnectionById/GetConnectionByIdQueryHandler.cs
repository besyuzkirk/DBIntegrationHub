using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Application.Connections.Queries.Dtos;
using DBIntegrationHub.Domain.Exceptions;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Connections.Queries.GetConnectionById;

public class GetConnectionByIdQueryHandler : IQueryHandler<GetConnectionByIdQuery, ConnectionDto>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IEncryptionService _encryptionService;

    public GetConnectionByIdQueryHandler(
        IConnectionRepository connectionRepository,
        IEncryptionService encryptionService)
    {
        _connectionRepository = connectionRepository;
        _encryptionService = encryptionService;
    }

    public async Task<Result<ConnectionDto>> Handle(GetConnectionByIdQuery request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (connection == null)
        {
            return Result.Failure<ConnectionDto>($"Connection bulunamadı. Id: {request.Id}");
        }

        // Connection string'i decrypt et ve maskele
        var decryptedConnectionString = _encryptionService.Decrypt(connection.ConnectionString);
        var maskedConnectionString = _encryptionService.MaskConnectionString(decryptedConnectionString);

        var dto = new ConnectionDto(
            connection.Id,
            connection.Name,
            maskedConnectionString, // Şifreler maskelenmiş halde döner (Password=***)
            connection.DatabaseType,
            connection.IsActive,
            connection.CreatedAt);

        return Result.Success(dto);
    }
}

