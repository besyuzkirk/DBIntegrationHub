using DBIntegrationHub.Application.Abstractions.Data;
using DBIntegrationHub.Application.Connections.Queries.GetConnectionSchema;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using System.Data;
using System.Data.Common;

namespace DBIntegrationHub.Infrastructure.Data;

public class SchemaService : ISchemaService
{
    private readonly ILogger<SchemaService> _logger;

    public SchemaService(ILogger<SchemaService> logger)
    {
        _logger = logger;
    }

    public async Task<ConnectionSchemaResponse> GetDatabaseSchemaAsync(
        string databaseType,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            DbConnection connection = databaseType.ToLower() switch
            {
                "postgresql" => new NpgsqlConnection(connectionString),
                "mysql" => new MySqlConnection(connectionString),
                "sqlserver" => new SqlConnection(connectionString),
                _ => throw new NotSupportedException($"Desteklenmeyen veritabanı tipi: {databaseType}")
            };

            await using (connection)
            {
                await connection.OpenAsync(cancellationToken);
                
                var tables = await GetTablesAsync(connection, databaseType, cancellationToken);
                
                _logger.LogInformation(
                    "Schema bilgileri başarıyla alındı - Tip: {DatabaseType}, Tablo Sayısı: {TableCount}",
                    databaseType,
                    tables.Count);

                return new ConnectionSchemaResponse(tables);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schema bilgileri alınırken hata oluştu - Tip: {DatabaseType}", databaseType);
            throw;
        }
    }

    private async Task<List<TableInfo>> GetTablesAsync(
        DbConnection connection, 
        string databaseType, 
        CancellationToken cancellationToken)
    {
        var tables = new List<TableInfo>();

        // Tablo listesini al
        var tableNames = await GetTableNamesAsync(connection, databaseType, cancellationToken);

        // SQL Server için sadece temel bilgileri al, diğerleri için tam bilgi
        if (databaseType.ToLower() == "sqlserver")
        {
            // SQL Server için sadece tablo adları, kolon bilgileri olmadan
            foreach (var tableName in tableNames)
            {
                tables.Add(new TableInfo(tableName.Name, tableName.Schema, new List<ColumnInfo>()));
            }
        }
        else
        {
            // PostgreSQL ve MySQL için tam kolon bilgileri
            foreach (var tableName in tableNames)
            {
                var columns = await GetColumnsAsync(connection, tableName, databaseType, cancellationToken);
                tables.Add(new TableInfo(tableName.Name, tableName.Schema, columns));
            }
        }

        return tables;
    }

    private async Task<List<(string Name, string Schema)>> GetTableNamesAsync(
        DbConnection connection, 
        string databaseType, 
        CancellationToken cancellationToken)
    {
        var query = databaseType.ToLower() switch
        {
            "postgresql" => @"
                SELECT table_name, table_schema 
                FROM information_schema.tables 
                WHERE table_schema NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                AND table_type = 'BASE TABLE'
                ORDER BY table_schema, table_name",
            "mysql" => @"
                SELECT table_name, table_schema 
                FROM information_schema.tables 
                WHERE table_schema NOT IN ('information_schema', 'performance_schema', 'mysql', 'sys')
                AND table_type = 'BASE TABLE'
                ORDER BY table_schema, table_name",
            "sqlserver" => @"
                SELECT t.name as table_name, s.name as table_schema
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                ORDER BY s.name, t.name",
            _ => throw new NotSupportedException($"Desteklenmeyen veritabanı tipi: {databaseType}")
        };

        await using var command = connection.CreateCommand();
        command.CommandText = query;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var tables = new List<(string Name, string Schema)>();

        while (await reader.ReadAsync(cancellationToken))
        {
            tables.Add((reader.GetString(0), reader.GetString(1)));
        }

        return tables;
    }

    private async Task<List<ColumnInfo>> GetColumnsAsync(
        DbConnection connection, 
        (string Name, string Schema) table, 
        string databaseType, 
        CancellationToken cancellationToken)
    {
        var query = databaseType.ToLower() switch
        {
            "postgresql" => @"
                SELECT 
                    column_name,
                    data_type,
                    is_nullable,
                    CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key,
                    character_maximum_length,
                    numeric_precision,
                    numeric_scale
                FROM information_schema.columns c
                LEFT JOIN (
                    SELECT ku.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage ku 
                        ON tc.constraint_name = ku.constraint_name
                    WHERE tc.table_name = @table_name 
                        AND tc.table_schema = @table_schema
                        AND tc.constraint_type = 'PRIMARY KEY'
                ) pk ON c.column_name = pk.column_name
                WHERE c.table_name = @table_name 
                    AND c.table_schema = @table_schema
                ORDER BY c.ordinal_position",
            "mysql" => @"
                SELECT 
                    column_name,
                    data_type,
                    is_nullable,
                    CASE WHEN column_key = 'PRI' THEN true ELSE false END as is_primary_key,
                    character_maximum_length,
                    numeric_precision,
                    numeric_scale
                FROM information_schema.columns
                WHERE table_name = @table_name 
                    AND table_schema = @table_schema
                ORDER BY ordinal_position",
            "sqlserver" => @"
                SELECT 
                    c.name as column_name,
                    t.name as data_type,
                    c.is_nullable,
                    0 as is_primary_key,
                    c.max_length as character_maximum_length,
                    c.precision as numeric_precision,
                    c.scale as numeric_scale
                FROM sys.columns c
                INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                INNER JOIN sys.tables tb ON c.object_id = tb.object_id
                INNER JOIN sys.schemas s ON tb.schema_id = s.schema_id
                WHERE tb.name = @table_name 
                    AND s.name = @table_schema
                ORDER BY c.column_id",
            _ => throw new NotSupportedException($"Desteklenmeyen veritabanı tipi: {databaseType}")
        };

        await using var command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = 60; // 60 saniye timeout
        
        // Parametreleri ekle
        var tableNameParam = command.CreateParameter();
        tableNameParam.ParameterName = "@table_name";
        tableNameParam.Value = table.Name;
        command.Parameters.Add(tableNameParam);

        var schemaParam = command.CreateParameter();
        schemaParam.ParameterName = "@table_schema";
        schemaParam.Value = table.Schema;
        command.Parameters.Add(schemaParam);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var columns = new List<ColumnInfo>();

        while (await reader.ReadAsync(cancellationToken))
        {
            // SQL Server'da is_nullable boolean, diğerlerinde string
            bool isNullable;
            if (databaseType.ToLower() == "sqlserver")
            {
                isNullable = reader.GetBoolean(2); // is_nullable kolonu (boolean)
            }
            else
            {
                var isNullableValue = reader.GetString(2); // is_nullable kolonu (string)
                isNullable = isNullableValue == "YES" || isNullableValue == "1" || isNullableValue == "TRUE";
            }
            
            // SQL Server'da is_primary_key Int32, diğerlerinde boolean
            bool isPrimaryKey;
            if (databaseType.ToLower() == "sqlserver")
            {
                isPrimaryKey = Convert.ToInt32(reader.GetValue(3)) == 1; // is_primary_key kolonu (Int32/Int16)
            }
            else
            {
                isPrimaryKey = reader.GetBoolean(3); // is_primary_key kolonu (Boolean)
            }
            
            // Güvenli sayısal değer okuma
            int? maxLength = reader.IsDBNull(4) ? null : Convert.ToInt32(reader.GetValue(4));
            int? precision = reader.IsDBNull(5) ? null : Convert.ToInt32(reader.GetValue(5));
            int? scale = reader.IsDBNull(6) ? null : Convert.ToInt32(reader.GetValue(6));
            
            columns.Add(new ColumnInfo(
                reader.GetString(0), // column_name
                reader.GetString(1), // data_type
                isNullable,
                isPrimaryKey,
                maxLength, // character_maximum_length
                precision, // numeric_precision
                scale     // numeric_scale
            ));
        }

        return columns;
    }

    public async Task<List<ColumnInfo>> GetTableColumnsAsync(
        string databaseType,
        string connectionString,
        string tableName,
        string schema,
        CancellationToken cancellationToken = default)
    {
        try
        {
            DbConnection connection = databaseType.ToLower() switch
            {
                "postgresql" => new NpgsqlConnection(connectionString),
                "mysql" => new MySqlConnection(connectionString),
                "sqlserver" => new SqlConnection(connectionString),
                _ => throw new NotSupportedException($"Desteklenmeyen veritabanı tipi: {databaseType}")
            };

            await using (connection)
            {
                await connection.OpenAsync(cancellationToken);
                
                var columns = await GetColumnsAsync(connection, (tableName, schema), databaseType, cancellationToken);
                
                _logger.LogInformation(
                    "Tablo kolonları başarıyla alındı - Tip: {DatabaseType}, Tablo: {TableName}, Kolon Sayısı: {ColumnCount}",
                    databaseType,
                    tableName,
                    columns.Count);

                return columns;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tablo kolonları alınırken hata oluştu - Tip: {DatabaseType}, Tablo: {TableName}", databaseType, tableName);
            throw;
        }
    }
}
