using DBIntegrationHub.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using System.Data.Common;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DBIntegrationHub.Infrastructure.Data;

public class IntegrationRunner : IIntegrationRunner
{
    private readonly ILogger<IntegrationRunner> _logger;
    private readonly ITypeMapper _typeMapper;

    public IntegrationRunner(
        ILogger<IntegrationRunner> logger,
        ITypeMapper typeMapper)
    {
        _logger = logger;
        _typeMapper = typeMapper;
    }

    public async Task<IntegrationRunResult> RunAsync(
        string sourceDatabaseType,
        string sourceConnectionString,
        string sourceQuery,
        string targetDatabaseType,
        string targetConnectionString,
        string targetQuery,
        Dictionary<string, string>? columnMappings = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Source'tan veri çek
            var sourceData = await FetchSourceDataAsync(
                sourceDatabaseType,
                sourceConnectionString,
                sourceQuery,
                cancellationToken);

            if (sourceData.Count == 0)
            {
                stopwatch.Stop();
                return new IntegrationRunResult(
                    Success: true,
                    RowsAffected: 0,
                    DurationMs: stopwatch.ElapsedMilliseconds,
                    Message: "Source'ta veri bulunamadı.");
            }

            // Target'a veri yaz
            var rowsAffected = await InsertTargetDataAsync(
                targetDatabaseType,
                targetConnectionString,
                targetQuery,
                sourceData,
                columnMappings,
                cancellationToken);

            stopwatch.Stop();

            return new IntegrationRunResult(
                Success: true,
                RowsAffected: rowsAffected,
                DurationMs: stopwatch.ElapsedMilliseconds,
                Message: $"Integration başarıyla tamamlandı. {rowsAffected} satır etkilendi.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Integration çalıştırılırken hata oluştu");

            return new IntegrationRunResult(
                Success: false,
                RowsAffected: 0,
                DurationMs: stopwatch.ElapsedMilliseconds,
                Error: ex.Message);
        }
    }

    private async Task<List<Dictionary<string, object?>>> FetchSourceDataAsync(
        string databaseType,
        string connectionString,
        string query,
        CancellationToken cancellationToken)
    {
        DbConnection connection = databaseType.ToLower() switch
        {
            "postgresql" => new NpgsqlConnection(connectionString),
            "mysql" => new MySqlConnection(connectionString),
            "sqlserver" => new SqlConnection(connectionString),
            _ => throw new NotSupportedException($"Desteklenmeyen veritabanı tipi: {databaseType}")
        };

        var result = new List<Dictionary<string, object?>>();

        await using (connection)
        {
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandTimeout = 300; // 5 dakika

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value;
                }

                result.Add(row);
            }
        }

        _logger.LogInformation("Source'tan {Count} satır okundu", result.Count);

        return result;
    }

    private async Task<int> InsertTargetDataAsync(
        string databaseType,
        string connectionString,
        string query,
        List<Dictionary<string, object?>> sourceData,
        Dictionary<string, string>? columnMappings,
        CancellationToken cancellationToken)
    {
        DbConnection connection = databaseType.ToLower() switch
        {
            "postgresql" => new NpgsqlConnection(connectionString),
            "mysql" => new MySqlConnection(connectionString),
            "sqlserver" => new SqlConnection(connectionString),
            _ => throw new NotSupportedException($"Desteklenmeyen veritabanı tipi: {databaseType}")
        };

        int totalRowsAffected = 0;

        await using (connection)
        {
            await connection.OpenAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var row in sourceData)
                {
                    await using var command = connection.CreateCommand();
                    command.Transaction = (DbTransaction)transaction;
                    command.CommandText = query;
                    command.CommandTimeout = 300;

                    // Parametreleri ekle (mapping ile)
                    AddParametersToCommand(command, row, columnMappings, databaseType);

                    var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                    totalRowsAffected += rowsAffected;
                }

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Target'a {Count} satır yazıldı", totalRowsAffected);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        return totalRowsAffected;
    }

    private void AddParametersToCommand(
        DbCommand command,
        Dictionary<string, object?> rowData,
        Dictionary<string, string>? columnMappings,
        string databaseType)
    {
        // Query'deki parametreleri bul
        var parameterMatches = Regex.Matches(command.CommandText, @"[@:]\w+");
        var parameterNames = parameterMatches
            .Select(m => m.Value.TrimStart('@', ':'))
            .Distinct()
            .ToList();

        foreach (var paramName in parameterNames)
        {
            var parameter = command.CreateParameter();
            
            // Parametre adını formatla
            parameter.ParameterName = databaseType.ToLower() switch
            {
                "postgresql" => paramName,
                "mysql" => $"@{paramName}",
                "sqlserver" => $"@{paramName}",
                _ => $"@{paramName}"
            };

            // Mapping varsa kullan, yoksa otomatik eşleştir
            string? sourceColumnName = null;

            if (columnMappings != null && columnMappings.TryGetValue(paramName, out var mappedColumn))
            {
                // Manuel mapping var
                sourceColumnName = mappedColumn;
            }
            else
            {
                // Otomatik eşleştirme (case-insensitive)
                sourceColumnName = rowData.Keys.FirstOrDefault(
                    k => k.Equals(paramName, StringComparison.OrdinalIgnoreCase));
            }

            if (sourceColumnName != null && rowData.TryGetValue(sourceColumnName, out var value))
            {
                // TIP DÖNÜŞÜMÜ: TypeMapper ile otomatik dönüşüm yap
                try
                {
                    if (value != null && value != DBNull.Value)
                    {
                        // Hedef tip belirlenemediği için value tipini koruyoruz
                        // Ama yine de typeMapper'dan geçiriyoruz (format dönüşümleri için)
                        var convertedValue = _typeMapper.ConvertValue(
                            value,
                            value.GetType(), // Kaynak tipte kal
                            "source", // Kaynak DB tipi (gerçek değer runtime'da belli olacak)
                            databaseType);
                        
                        parameter.Value = convertedValue ?? DBNull.Value;
                    }
                    else
                    {
                        parameter.Value = DBNull.Value;
                    }
                }
                catch (TypeConversionException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Tip dönüşümü başarısız, orijinal değer kullanılıyor - Parametre: {ParamName}, Değer: {Value}",
                        paramName,
                        value);
                    
                    // Dönüşüm başarısız olursa orijinal değeri kullan
                    parameter.Value = value ?? DBNull.Value;
                }
            }
            else
            {
                parameter.Value = DBNull.Value;
                _logger.LogWarning(
                    "Parametre için kolon bulunamadı - Parametre: {ParamName}, Mapping: {Mapping}",
                    paramName,
                    sourceColumnName ?? "null");
            }

            command.Parameters.Add(parameter);
        }
    }
}

