using DBIntegrationHub.Application.Abstractions.Security;
using Microsoft.AspNetCore.DataProtection;
using System.Text.RegularExpressions;

namespace DBIntegrationHub.Infrastructure.Security;

/// <summary>
/// .NET Data Protection API kullanarak şifreleme servisi
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        // "DBIntegrationHub.ConnectionStrings" purpose'u ile data protector oluştur
        // Bu, bu amaç için özel bir encryption key kullanılmasını sağlar
        _protector = dataProtectionProvider.CreateProtector("DBIntegrationHub.ConnectionStrings");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return plainText;

        try
        {
            return _protector.Protect(plainText);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Connection string şifrelenemedi", ex);
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
            return encryptedText;

        try
        {
            return _protector.Unprotect(encryptedText);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Connection string şifresi çözülemedi. Veri bozuk olabilir.", ex);
        }
    }

    public string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        // Password, PWD, Pwd gibi ifadeleri maskele
        var maskedString = Regex.Replace(
            connectionString,
            @"(Password|Pwd|pwd)=([^;]+)",
            "$1=***",
            RegexOptions.IgnoreCase);

        // API Key, Secret gibi ifadeleri de maskele
        maskedString = Regex.Replace(
            maskedString,
            @"(ApiKey|Secret|Token)=([^;]+)",
            "$1=***",
            RegexOptions.IgnoreCase);

        // MongoDB connection string için özel format
        // mongodb://username:password@host:port -> mongodb://username:***@host:port
        maskedString = Regex.Replace(
            maskedString,
            @"(mongodb(?:\+srv)?://[^:]+:)([^@]+)(@)",
            "$1***$3",
            RegexOptions.IgnoreCase);

        return maskedString;
    }
}

