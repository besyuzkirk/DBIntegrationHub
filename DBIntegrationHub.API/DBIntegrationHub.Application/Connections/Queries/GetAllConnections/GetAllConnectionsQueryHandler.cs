using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Application.Connections.Queries.Dtos;
using DBIntegrationHub.Domain.Repositories;
using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Application.Connections.Queries.GetAllConnections;

public class GetAllConnectionsQueryHandler : IQueryHandler<GetAllConnectionsQuery, IEnumerable<ConnectionDto>>
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IEncryptionService _encryptionService;

    public GetAllConnectionsQueryHandler(
        IConnectionRepository connectionRepository,
        IEncryptionService encryptionService)
    {
        _connectionRepository = connectionRepository;
        _encryptionService = encryptionService;
    }

    public async Task<Result<IEnumerable<ConnectionDto>>> Handle(
        GetAllConnectionsQuery request,
        CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetAllAsync(cancellationToken);

        var dtos = connections.Select(c =>
        {
            // Connection string'i decrypt et ve maskele
            var decryptedConnectionString = _encryptionService.Decrypt(c.ConnectionString);
            var maskedConnectionString = _encryptionService.MaskConnectionString(decryptedConnectionString);

            return new ConnectionDto(
                c.Id,
                c.Name,
                maskedConnectionString, // Şifreler maskelenmiş halde döner (Password=***)
                c.DatabaseType,
                c.IsActive,
                c.CreatedAt);
        });

        return Result.Success(dtos);
    }
}

