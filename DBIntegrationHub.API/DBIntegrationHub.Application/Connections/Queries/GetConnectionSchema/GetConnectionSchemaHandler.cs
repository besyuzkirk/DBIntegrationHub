using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;

public class GetConnectionSchemaHandler : IQueryHandler<GetConnectionSchemaQuery, ConnectionSchemaResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchemaService _schemaService;
    private readonly IEncryptionService _encryptionService;

    public GetConnectionSchemaHandler(
        IApplicationDbContext context,
        ISchemaService schemaService,
        IEncryptionService encryptionService)
    {
        _context = context;
        _schemaService = schemaService;
        _encryptionService = encryptionService;
    }

    public async Task<Result<ConnectionSchemaResponse>> Handle(
        GetConnectionSchemaQuery request,
        CancellationToken cancellationToken)
    {
        var connection = await _context.Connections
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);

        if (connection == null)
        {
            return Result.Failure<ConnectionSchemaResponse>("Bağlantı bulunamadı");
        }

        if (!connection.IsActive)
        {
            return Result.Failure<ConnectionSchemaResponse>("Bağlantı aktif değil");
        }

        try
        {
            // Connection string'i decrypt et
            var decryptedConnectionString = _encryptionService.Decrypt(connection.ConnectionString);
            
            // Gerçek schema bilgilerini al
            var schemaInfo = await _schemaService.GetDatabaseSchemaAsync(
                connection.DatabaseType, 
                decryptedConnectionString, 
                cancellationToken);
            
            return Result.Success(schemaInfo);
        }
        catch (Exception ex)
        {
            return Result.Failure<ConnectionSchemaResponse>($"Schema bilgileri alınamadı: {ex.Message}");
        }
    }

}
