namespace DBIntegrationHub.Application.Abstractions.Data;

/// <summary>
/// Database tipleri arasında otomatik dönüşüm için servis
/// Mapping sırasında tip uyumsuzluklarını çözer
/// </summary>
public interface ITypeMapper
{
    /// <summary>
    /// Değeri kaynak tipten hedef tipe dönüştürür
    /// </summary>
    /// <param name="value">Dönüştürülecek değer</param>
    /// <param name="targetType">Hedef C# tipi (örn: typeof(int))</param>
    /// <param name="sourceDatabaseType">Kaynak database tipi (PostgreSQL, MySQL, SQLServer)</param>
    /// <param name="targetDatabaseType">Hedef database tipi</param>
    /// <returns>Dönüştürülmüş değer</returns>
    object? ConvertValue(
        object? value, 
        Type targetType, 
        string sourceDatabaseType, 
        string targetDatabaseType);

    /// <summary>
    /// Değeri kaynak tipten hedef tipe dönüştürür (generic)
    /// </summary>
    T? ConvertValue<T>(
        object? value, 
        string sourceDatabaseType, 
        string targetDatabaseType);

    /// <summary>
    /// İki database tipi arasında belirli bir tip için dönüşüm gerekli mi kontrol eder
    /// </summary>
    bool RequiresConversion(
        Type type, 
        string sourceDatabaseType, 
        string targetDatabaseType);

    /// <summary>
    /// Custom converter kaydet (extensibility için)
    /// </summary>
    void RegisterConverter<TSource, TTarget>(Func<TSource, TTarget> converter);

    /// <summary>
    /// Null değer için default değer döndürür (hedef tipe göre)
    /// </summary>
    object? GetDefaultValue(Type type);
}

/// <summary>
/// Tip dönüşümü sırasında oluşan hatalar için
/// </summary>
public class TypeConversionException : Exception
{
    public object? SourceValue { get; }
    public Type? SourceType { get; }
    public Type? TargetType { get; }
    public string SourceDatabase { get; }
    public string TargetDatabase { get; }

    public TypeConversionException(
        string message,
        object? sourceValue,
        Type? sourceType,
        Type? targetType,
        string sourceDatabase,
        string targetDatabase,
        Exception? innerException = null)
        : base(message, innerException)
    {
        SourceValue = sourceValue;
        SourceType = sourceType;
        TargetType = targetType;
        SourceDatabase = sourceDatabase;
        TargetDatabase = targetDatabase;
    }
}

