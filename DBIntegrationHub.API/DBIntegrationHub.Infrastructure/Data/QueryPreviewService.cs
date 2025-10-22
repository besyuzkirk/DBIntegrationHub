using DBIntegrationHub.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace DBIntegrationHub.Infrastructure.Data;

public class QueryPreviewService : IQueryPreviewService
{
    private readonly ILogger<QueryPreviewService> _logger;

    public QueryPreviewService(ILogger<QueryPreviewService> logger)
    {
        _logger = logger;
    }

    public async Task<List<string>> GetColumnNamesAsync(
        string databaseType,
        string connectionString,
        string query,
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
                
                await using var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = 30;

                // CommandBehavior.SchemaOnly: Sorguyu çalıştırmadan sadece şema bilgisini getirir
                await using var reader = await command.ExecuteReaderAsync(
                    CommandBehavior.SchemaOnly, 
                    cancellationToken);

                var schemaTable = reader.GetSchemaTable();
                if (schemaTable == null)
                {
                    _logger.LogWarning("Schema table is null for query: {Query}", query);
                    return new List<string>();
                }

                var columns = schemaTable.Rows
                    .Cast<DataRow>()
                    .Select(r => r["ColumnName"]?.ToString() ?? string.Empty)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();

                _logger.LogInformation(
                    "Kolon isimleri alındı - Veritabanı: {DatabaseType}, Kolon Sayısı: {Count}",
                    databaseType,
                    columns.Count);

                return columns;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Kolon isimleri alınamadı - Veritabanı: {DatabaseType}",
                databaseType);

            throw new InvalidOperationException(
                $"Sorgudan kolon isimleri alınamadı: {ex.Message}", 
                ex);
        }
    }

    public async Task<QueryPreviewData> GetPreviewDataAsync(
        string databaseType,
        string connectionString,
        string query,
        int maxRows = 100,
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

                await using var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = 30;

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                // Kolonları al
                var columns = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                // Satırları oku (max limit)
                var rows = new List<Dictionary<string, object?>>();
                int rowCount = 0;
                while (await reader.ReadAsync(cancellationToken) && rowCount < maxRows)
                {
                    var row = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[columns[i]] = value;
                    }
                    rows.Add(row);
                    rowCount++;
                }

                _logger.LogInformation(
                    "Query preview başarılı - Veritabanı: {DatabaseType}, Satır: {RowCount}",
                    databaseType,
                    rowCount);

                return new QueryPreviewData(columns, rows, rowCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Query preview hatası - Veritabanı: {DatabaseType}",
                databaseType);

            throw new InvalidOperationException(
                $"Query preview alınamadı: {ex.Message}",
                ex);
        }
    }

    public List<string> ExtractParametersFromQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<string>();

        try
        {
            // @parameterName veya :parameterName formatındaki parametreleri bul
            var matches = Regex.Matches(query, @"[@:]\w+");
            
            var parameters = matches
                .Select(m => m.Value.TrimStart('@', ':')) // @ veya : işaretini kaldır
                .Distinct()
                .ToList();

            _logger.LogInformation(
                "Parametreler çıkarıldı - Parametre Sayısı: {Count}",
                parameters.Count);

            return parameters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parametreler çıkarılamadı");
            return new List<string>();
        }
    }
}

