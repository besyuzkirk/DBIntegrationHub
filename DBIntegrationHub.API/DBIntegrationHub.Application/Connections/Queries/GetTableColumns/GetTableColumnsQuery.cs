using DBIntegrationHub.Application.Abstractions.Messaging;

namespace DBIntegrationHub.Application.Connections.Queries.GetTableColumns;

public record GetTableColumnsQuery(
    Guid ConnectionId, 
    string TableName, 
    string Schema = "dbo") : IQuery<TableColumnsResponse>;
