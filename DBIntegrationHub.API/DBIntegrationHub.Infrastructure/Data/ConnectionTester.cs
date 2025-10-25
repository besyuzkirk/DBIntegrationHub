using DBIntegrationHub.Application.Abstractions.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MySqlConnector;
using Npgsql;
using System.Data.Common;
using System.Diagnostics;

namespace DBIntegrationHub.Infrastructure.Data;

public class ConnectionTester : IConnectionTester
{
    private readonly ILogger<ConnectionTester> _logger;

    public ConnectionTester(ILogger<ConnectionTester> logger)
    {
        _logger = logger;
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(
        string databaseType,
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (databaseType.ToLower() == "mongodb")
            {
                return await TestMongoDBConnectionAsync(connectionString, stopwatch, cancellationToken);
            }
            else
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
                    stopwatch.Stop();

                    _logger.LogInformation(
                        "Bağlantı testi başarılı - Tip: {DatabaseType}, Süre: {Duration}ms",
                        databaseType,
                        stopwatch.ElapsedMilliseconds);

                    return new ConnectionTestResult(
                        true,
                        "Bağlantı başarılı",
                        (int)stopwatch.ElapsedMilliseconds);
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Bağlantı testi başarısız - Tip: {DatabaseType}",
                databaseType);

            return new ConnectionTestResult(
                false,
                $"Bağlantı başarısız: {ex.Message}");
        }
    }

    private async Task<ConnectionTestResult> TestMongoDBConnectionAsync(
        string connectionString, 
        Stopwatch stopwatch, 
        CancellationToken cancellationToken)
    {
        try
        {
            var client = new MongoClient(connectionString);
            
            // Ping test
            await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1), 
                cancellationToken: cancellationToken);
            
            stopwatch.Stop();

            _logger.LogInformation(
                "MongoDB bağlantı testi başarılı - Süre: {Duration}ms",
                stopwatch.ElapsedMilliseconds);

            return new ConnectionTestResult(
                true,
                "MongoDB bağlantısı başarılı",
                (int)stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "MongoDB bağlantı testi başarısız");

            return new ConnectionTestResult(
                false,
                $"MongoDB bağlantısı başarısız: {ex.Message}");
        }
    }
}

