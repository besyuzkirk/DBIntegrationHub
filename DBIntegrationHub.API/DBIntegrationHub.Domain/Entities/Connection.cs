using DBIntegrationHub.Domain.Shared;

namespace DBIntegrationHub.Domain.Entities;

public class Connection : Entity
{
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Şifrelenmiş connection string. Direkt kullanılmamalı, GetDecryptedConnectionString() kullanılmalı.
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;
    
    public string DatabaseType { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Connection() { } // EF Core için

    private Connection(string name, string connectionString, string databaseType)
    {
        Name = name;
        ConnectionString = connectionString;
        DatabaseType = databaseType;
        IsActive = true;
    }

    /// <summary>
    /// Yeni connection oluşturur. ConnectionString şifrelenmiş olarak gelmelidir.
    /// </summary>
    public static Connection Create(string name, string encryptedConnectionString, string databaseType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection adı boş olamaz", nameof(name));

        if (string.IsNullOrWhiteSpace(encryptedConnectionString))
            throw new ArgumentException("Connection string boş olamaz", nameof(encryptedConnectionString));

        if (string.IsNullOrWhiteSpace(databaseType))
            throw new ArgumentException("Veritabanı tipi boş olamaz", nameof(databaseType));

        return new Connection(name, encryptedConnectionString, databaseType);
    }

    /// <summary>
    /// Connection'ı günceller. ConnectionString şifrelenmiş olarak gelmelidir.
    /// </summary>
    public void Update(string name, string encryptedConnectionString, string databaseType)
    {
        Name = name;
        ConnectionString = encryptedConnectionString;
        DatabaseType = databaseType;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

