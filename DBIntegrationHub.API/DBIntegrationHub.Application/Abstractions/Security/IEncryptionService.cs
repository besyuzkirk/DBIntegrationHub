namespace DBIntegrationHub.Application.Abstractions.Security;

/// <summary>
/// Hassas verilerin şifrelenmesi ve şifresinin çözülmesi için servis
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Verilen metni şifreler
    /// </summary>
    string Encrypt(string plainText);

    /// <summary>
    /// Şifrelenmiş metni çözer
    /// </summary>
    string Decrypt(string encryptedText);

    /// <summary>
    /// Connection string'deki hassas bilgileri maskeler (API response için)
    /// Örnek: "Password=12345" -> "Password=***"
    /// </summary>
    string MaskConnectionString(string connectionString);
}

