using DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;

namespace DBIntegrationHub.Application.Connections.Queries.GetTableColumns;

public record TableColumnsResponse(List<ColumnInfo> Columns);
