using DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;
using DBIntegrationHub.Application.Connections.Queries.GetTableColumns;

namespace DBIntegrationHub.Application.Abstractions.Data;

public interface ISchemaService
{
    Task<ConnectionSchemaResponse> GetDatabaseSchemaAsync(
        string databaseType,
        string connectionString,
        CancellationToken cancellationToken = default);
    
    Task<List<ColumnInfo>> GetTableColumnsAsync(
        string databaseType, 
        string connectionString, 
        string tableName, 
        string schema, 
        CancellationToken cancellationToken = default);
}
