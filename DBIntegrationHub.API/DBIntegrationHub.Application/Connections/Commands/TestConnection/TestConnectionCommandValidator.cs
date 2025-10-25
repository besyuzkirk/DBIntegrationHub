using FluentValidation;

namespace DBIntegrationHub.Application.Connections.Commands.TestConnection;

public class TestConnectionCommandValidator : AbstractValidator<TestConnectionCommand>
{
    public TestConnectionCommandValidator()
    {
        RuleFor(x => x.DatabaseType)
            .NotEmpty().WithMessage("Veritabanı tipi boş olamaz")
            .Must(BeValidDatabaseType).WithMessage("Geçerli bir veritabanı tipi seçiniz (PostgreSQL, MySQL, SQLServer, MongoDB)");

        RuleFor(x => x.ConnectionString)
            .NotEmpty().WithMessage("Connection string boş olamaz")
            .Must((command, connectionString) => IsValidConnectionString(command.DatabaseType, connectionString))
            .WithMessage((command, connectionString) => GetConnectionStringErrorMessage(command.DatabaseType, connectionString));
    }

    private bool BeValidDatabaseType(string databaseType)
    {
        var validTypes = new[] { "PostgreSQL", "MySQL", "SQLServer", "MongoDB" };
        return validTypes.Contains(databaseType, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsValidConnectionString(string databaseType, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        return databaseType.ToLower() switch
        {
            "postgresql" => IsValidPostgreSQLConnectionString(connectionString),
            "mysql" => IsValidMySQLConnectionString(connectionString),
            "sqlserver" => IsValidSQLServerConnectionString(connectionString),
            "mongodb" => IsValidMongoDBConnectionString(connectionString),
            _ => false
        };
    }

    private bool IsValidPostgreSQLConnectionString(string connectionString)
    {
        var requiredKeys = new[] { "Host", "Database", "Username", "Password" };
        var hasAllKeys = requiredKeys.All(key => connectionString.Contains($"{key}=", StringComparison.OrdinalIgnoreCase));
        
        // PostgreSQL için yanlış anahtar kelimeleri kontrol et
        var wrongKeys = new[] { "Server=", "Uid=", "Pwd=", "User Id=" };
        var hasWrongKeys = wrongKeys.Any(key => connectionString.Contains(key, StringComparison.OrdinalIgnoreCase));
        
        return hasAllKeys && !hasWrongKeys;
    }

    private bool IsValidMySQLConnectionString(string connectionString)
    {
        var requiredKeys = new[] { "Server", "Database", "Uid", "Pwd" };
        var hasAllKeys = requiredKeys.All(key => connectionString.Contains($"{key}=", StringComparison.OrdinalIgnoreCase));
        
        // MySQL için yanlış anahtar kelimeleri kontrol et
        var wrongKeys = new[] { "Host=", "Username=", "Password=", "User Id=" };
        var hasWrongKeys = wrongKeys.Any(key => connectionString.Contains(key, StringComparison.OrdinalIgnoreCase));
        
        return hasAllKeys && !hasWrongKeys;
    }

    private bool IsValidSQLServerConnectionString(string connectionString)
    {
        var requiredKeys = new[] { "Server", "Database", "User Id", "Password" };
        var hasAllKeys = requiredKeys.All(key => connectionString.Contains($"{key}=", StringComparison.OrdinalIgnoreCase));
        
        // SQL Server için yanlış anahtar kelimeleri kontrol et
        var wrongKeys = new[] { "Host=", "Uid=", "Pwd=", "Username=" };
        var hasWrongKeys = wrongKeys.Any(key => connectionString.Contains(key, StringComparison.OrdinalIgnoreCase));
        
        return hasAllKeys && !hasWrongKeys;
    }

    private bool IsValidMongoDBConnectionString(string connectionString)
    {
        return connectionString.StartsWith("mongodb://", StringComparison.OrdinalIgnoreCase) ||
               connectionString.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase);
    }

    private string GetConnectionStringErrorMessage(string databaseType, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "Connection string boş olamaz";

        return databaseType.ToLower() switch
        {
            "postgresql" => GetPostgreSQLErrorMessage(connectionString),
            "mysql" => GetMySQLErrorMessage(connectionString),
            "sqlserver" => GetSQLServerErrorMessage(connectionString),
            "mongodb" => GetMongoDBErrorMessage(connectionString),
            _ => "Geçersiz veritabanı tipi"
        };
    }

    private string GetPostgreSQLErrorMessage(string connectionString)
    {
        var requiredKeys = new[] { "Host", "Database", "Username", "Password" };
        var missingKeys = requiredKeys.Where(key => !connectionString.Contains($"{key}=", StringComparison.OrdinalIgnoreCase)).ToList();
        
        var wrongKeys = new[] { "Server=", "Uid=", "Pwd=", "User Id=" };
        var foundWrongKeys = wrongKeys.Where(key => connectionString.Contains(key, StringComparison.OrdinalIgnoreCase)).ToList();

        if (missingKeys.Any())
            return $"PostgreSQL için eksik alanlar: {string.Join(", ", missingKeys)}";

        if (foundWrongKeys.Any())
            return $"PostgreSQL için yanlış anahtar kelimeler: {string.Join(", ", foundWrongKeys)}. Doğru format: Host=...;Database=...;Username=...;Password=...";

        return "Geçersiz PostgreSQL connection string formatı";
    }

    private string GetMySQLErrorMessage(string connectionString)
    {
        var requiredKeys = new[] { "Server", "Database", "Uid", "Pwd" };
        var missingKeys = requiredKeys.Where(key => !connectionString.Contains($"{key}=", StringComparison.OrdinalIgnoreCase)).ToList();
        
        var wrongKeys = new[] { "Host=", "Username=", "Password=", "User Id=" };
        var foundWrongKeys = wrongKeys.Where(key => connectionString.Contains(key, StringComparison.OrdinalIgnoreCase)).ToList();

        if (missingKeys.Any())
            return $"MySQL için eksik alanlar: {string.Join(", ", missingKeys)}";

        if (foundWrongKeys.Any())
            return $"MySQL için yanlış anahtar kelimeler: {string.Join(", ", foundWrongKeys)}. Doğru format: Server=...;Database=...;Uid=...;Pwd=...";

        return "Geçersiz MySQL connection string formatı";
    }

    private string GetSQLServerErrorMessage(string connectionString)
    {
        var requiredKeys = new[] { "Server", "Database", "User Id", "Password" };
        var missingKeys = requiredKeys.Where(key => !connectionString.Contains($"{key}=", StringComparison.OrdinalIgnoreCase)).ToList();
        
        var wrongKeys = new[] { "Host=", "Uid=", "Pwd=", "Username=" };
        var foundWrongKeys = wrongKeys.Where(key => connectionString.Contains(key, StringComparison.OrdinalIgnoreCase)).ToList();

        if (missingKeys.Any())
            return $"SQL Server için eksik alanlar: {string.Join(", ", missingKeys)}";

        if (foundWrongKeys.Any())
            return $"SQL Server için yanlış anahtar kelimeler: {string.Join(", ", foundWrongKeys)}. Doğru format: Server=...;Database=...;User Id=...;Password=...";

        return "Geçersiz SQL Server connection string formatı";
    }

    private string GetMongoDBErrorMessage(string connectionString)
    {
        if (!connectionString.StartsWith("mongodb://", StringComparison.OrdinalIgnoreCase) && 
            !connectionString.StartsWith("mongodb+srv://", StringComparison.OrdinalIgnoreCase))
        {
            return "MongoDB connection string 'mongodb://' veya 'mongodb+srv://' ile başlamalıdır";
        }

        return "Geçersiz MongoDB connection string formatı";
    }
}

