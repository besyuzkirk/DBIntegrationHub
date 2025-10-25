using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Abstractions.Messaging;
using DBIntegrationHub.Application.Abstractions.Security;
using DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;
using DBIntegrationHub.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace DBIntegrationHub.Application.Connections.Queries.GetTableColumns;

public class GetTableColumnsHandler : IQueryHandler<GetTableColumnsQuery, TableColumnsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ISchemaService _schemaService;
    private readonly IEncryptionService _encryptionService;

    public GetTableColumnsHandler(
        IApplicationDbContext context,
        ISchemaService schemaService,
        IEncryptionService encryptionService)
    {
        _context = context;
        _schemaService = schemaService;
        _encryptionService = encryptionService;
    }

    public async Task<Result<TableColumnsResponse>> Handle(
        GetTableColumnsQuery request,
        CancellationToken cancellationToken)
    {
        var connection = await _context.Connections
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);

        if (connection == null)
        {
            return Result.Failure<TableColumnsResponse>("Bağlantı bulunamadı");
        }

        if (!connection.IsActive)
        {
            return Result.Failure<TableColumnsResponse>("Bağlantı aktif değil");
        }

        try
        {
            // Connection string'i decrypt et
            var decryptedConnectionString = _encryptionService.Decrypt(connection.ConnectionString);
            
            // Sadece belirtilen tablo için kolon bilgilerini al
            var columns = await _schemaService.GetTableColumnsAsync(
                connection.DatabaseType, 
                decryptedConnectionString,
                request.TableName,
                request.Schema,
                cancellationToken);
            
            return Result.Success(new TableColumnsResponse(columns));
        }
        catch (Exception ex)
        {
            return Result.Failure<TableColumnsResponse>($"Kolon bilgileri alınamadı: {ex.Message}");
        }
    }
}
