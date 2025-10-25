namespace DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;

public record ConnectionSchemaResponse(
    List<TableInfo> Tables
);

public record TableInfo(
    string Name,
    string Schema,
    List<ColumnInfo> Columns
);

public record ColumnInfo(
    string Name,
    string DataType,
    bool IsNullable,
    bool IsPrimaryKey,
    int? MaxLength,
    int? NumericPrecision,
    int? NumericScale
);
