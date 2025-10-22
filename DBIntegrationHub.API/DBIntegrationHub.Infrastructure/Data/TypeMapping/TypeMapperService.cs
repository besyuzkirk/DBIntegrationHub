using DBIntegrationHub.Application.Abstractions.Data;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DBIntegrationHub.Infrastructure.Data.TypeMapping;

/// <summary>
/// Database tipleri arasında otomatik dönüşüm servisi
/// </summary>
public class TypeMapperService : ITypeMapper
{
    private readonly ILogger<TypeMapperService> _logger;
    private readonly Dictionary<(Type source, Type target), Delegate> _customConverters;

    public TypeMapperService(ILogger<TypeMapperService> logger)
    {
        _logger = logger;
        _customConverters = new Dictionary<(Type, Type), Delegate>();
    }

    public object? ConvertValue(
        object? value,
        Type targetType,
        string sourceDatabaseType,
        string targetDatabaseType)
    {
        // Null kontrolü
        if (value == null || value == DBNull.Value)
        {
            return GetDefaultValue(targetType);
        }

        var sourceType = value.GetType();

        // Aynı tip ise direkt dön
        if (sourceType == targetType || targetType.IsAssignableFrom(sourceType))
        {
            return value;
        }

        try
        {
            // Custom converter var mı kontrol et
            if (_customConverters.TryGetValue((sourceType, targetType), out var converter))
            {
                return converter.DynamicInvoke(value);
            }

            // Otomatik dönüşüm yap
            return ConvertValueInternal(value, sourceType, targetType, sourceDatabaseType, targetDatabaseType);
        }
        catch (Exception ex)
        {
            throw new TypeConversionException(
                $"Tip dönüşümü başarısız: {sourceType.Name} -> {targetType.Name}",
                value,
                sourceType,
                targetType,
                sourceDatabaseType,
                targetDatabaseType,
                ex);
        }
    }

    public T? ConvertValue<T>(object? value, string sourceDatabaseType, string targetDatabaseType)
    {
        var result = ConvertValue(value, typeof(T), sourceDatabaseType, targetDatabaseType);
        return result == null ? default : (T)result;
    }

    public bool RequiresConversion(Type type, string sourceDatabaseType, string targetDatabaseType)
    {
        // Aynı database ise dönüşüm gerekmez
        if (sourceDatabaseType.Equals(targetDatabaseType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Bazı tipler her zaman uyumludur
        if (type == typeof(string) || type == typeof(byte[]))
        {
            return false;
        }

        return true;
    }

    public void RegisterConverter<TSource, TTarget>(Func<TSource, TTarget> converter)
    {
        _customConverters[(typeof(TSource), typeof(TTarget))] = converter;
        _logger.LogInformation(
            "Custom converter kaydedildi: {Source} -> {Target}",
            typeof(TSource).Name,
            typeof(TTarget).Name);
    }

    public object? GetDefaultValue(Type type)
    {
        // Nullable tip mi?
        if (Nullable.GetUnderlyingType(type) != null || !type.IsValueType)
        {
            return null;
        }

        // Value type için default değer
        return Activator.CreateInstance(type);
    }

    private object ConvertValueInternal(
        object value,
        Type sourceType,
        Type targetType,
        string sourceDatabaseType,
        string targetDatabaseType)
    {
        // Nullable type handling
        var underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // String dönüşümleri
        if (targetType == typeof(string))
        {
            return ConvertToString(value, sourceType);
        }

        // Numeric dönüşümleri
        if (IsNumericType(underlyingTargetType))
        {
            return ConvertToNumeric(value, underlyingTargetType);
        }

        // DateTime dönüşümleri
        if (underlyingTargetType == typeof(DateTime) || underlyingTargetType == typeof(DateTimeOffset))
        {
            return ConvertToDateTime(value, underlyingTargetType, sourceDatabaseType, targetDatabaseType);
        }

        // Boolean dönüşümleri
        if (underlyingTargetType == typeof(bool))
        {
            return ConvertToBoolean(value, sourceDatabaseType);
        }

        // Guid dönüşümleri
        if (underlyingTargetType == typeof(Guid))
        {
            return ConvertToGuid(value);
        }

        // Byte array dönüşümleri
        if (underlyingTargetType == typeof(byte[]))
        {
            return ConvertToByteArray(value);
        }

        // Genel dönüşüm (Convert.ChangeType)
        try
        {
            return Convert.ChangeType(value, underlyingTargetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            _logger.LogWarning(
                "Standart dönüşüm başarısız, ToString kullanılıyor: {Value} ({SourceType} -> {TargetType})",
                value,
                sourceType.Name,
                targetType.Name);

            // Son çare: ToString ve parse
            var stringValue = value.ToString() ?? string.Empty;
            return ParseFromString(stringValue, underlyingTargetType);
        }
    }

    private string ConvertToString(object value, Type sourceType)
    {
        // DateTime için özel format
        if (value is DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        // Decimal için özel format
        if (value is decimal dec)
        {
            return dec.ToString(CultureInfo.InvariantCulture);
        }

        return value.ToString() ?? string.Empty;
    }

    private object ConvertToNumeric(object value, Type targetType)
    {
        // String'den numeric'e
        if (value is string str)
        {
            // Boş string kontrolü
            if (string.IsNullOrWhiteSpace(str))
            {
                return GetDefaultValue(targetType) ?? 0;
            }

            // Parse et
            return ParseNumeric(str, targetType);
        }

        // Boolean'dan numeric'e (0 veya 1)
        if (value is bool boolValue)
        {
            return Convert.ChangeType(boolValue ? 1 : 0, targetType);
        }

        // Diğer numeric tiplerden dönüşüm
        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    private object ConvertToDateTime(object value, Type targetType, string sourceDb, string targetDb)
    {
        if (value is string str)
        {
            // Çeşitli tarih formatlarını dene
            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "dd/MM/yyyy HH:mm:ss",
                "dd/MM/yyyy",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(str, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt))
                {
                    return targetType == typeof(DateTimeOffset)
                        ? new DateTimeOffset(dt)
                        : dt;
                }
            }

            // Format bulunamadı, genel parse dene
            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return targetType == typeof(DateTimeOffset)
                    ? new DateTimeOffset(parsed)
                    : parsed;
            }

            throw new FormatException($"Geçersiz tarih formatı: {str}");
        }

        // Numeric'ten (Unix timestamp olabilir)
        if (IsNumericType(value.GetType()))
        {
            var timestamp = Convert.ToInt64(value);
            var dt = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            return targetType == typeof(DateTimeOffset)
                ? new DateTimeOffset(dt)
                : dt;
        }

        // DateTime <-> DateTimeOffset dönüşümü
        if (value is DateTime dateTime)
        {
            return targetType == typeof(DateTimeOffset)
                ? new DateTimeOffset(dateTime)
                : dateTime;
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return targetType == typeof(DateTime)
                ? dateTimeOffset.DateTime
                : dateTimeOffset;
        }

        throw new InvalidCastException($"DateTime dönüşümü yapılamadı: {value.GetType().Name}");
    }

    private bool ConvertToBoolean(object value, string databaseType)
    {
        if (value is bool b)
            return b;

        if (value is string str)
        {
            // "true", "false", "yes", "no", "1", "0"
            return str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                   str.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                   str.Equals("1", StringComparison.OrdinalIgnoreCase);
        }

        // Numeric'ten boolean'a
        if (IsNumericType(value.GetType()))
        {
            var numValue = Convert.ToDouble(value);
            return Math.Abs(numValue) > 0.0001; // 0 değilse true
        }

        throw new InvalidCastException($"Boolean dönüşümü yapılamadı: {value}");
    }

    private Guid ConvertToGuid(object value)
    {
        if (value is Guid guid)
            return guid;

        if (value is string str)
            return Guid.Parse(str);

        if (value is byte[] bytes && bytes.Length == 16)
            return new Guid(bytes);

        throw new InvalidCastException($"Guid dönüşümü yapılamadı: {value.GetType().Name}");
    }

    private byte[] ConvertToByteArray(object value)
    {
        if (value is byte[] bytes)
            return bytes;

        if (value is string str)
            return Convert.FromBase64String(str);

        if (value is Guid guid)
            return guid.ToByteArray();

        throw new InvalidCastException($"Byte array dönüşümü yapılamadı: {value.GetType().Name}");
    }

    private object ParseNumeric(string str, Type targetType)
    {
        if (targetType == typeof(decimal))
            return decimal.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(double))
            return double.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(float))
            return float.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(long))
            return long.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(int))
            return int.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(short))
            return short.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(byte))
            return byte.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(ulong))
            return ulong.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(uint))
            return uint.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(ushort))
            return ushort.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(sbyte))
            return sbyte.Parse(str, CultureInfo.InvariantCulture);

        throw new NotSupportedException($"Desteklenmeyen numeric tip: {targetType.Name}");
    }

    private object ParseFromString(string str, Type targetType)
    {
        if (targetType == typeof(DateTime))
            return DateTime.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(DateTimeOffset))
            return DateTimeOffset.Parse(str, CultureInfo.InvariantCulture);
        if (targetType == typeof(Guid))
            return Guid.Parse(str);
        if (targetType == typeof(bool))
            return bool.Parse(str);

        throw new FormatException($"String'den {targetType.Name} dönüşümü yapılamadı: {str}");
    }

    private bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
               type == typeof(byte) || type == typeof(decimal) || type == typeof(double) ||
               type == typeof(float) || type == typeof(uint) || type == typeof(ulong) ||
               type == typeof(ushort) || type == typeof(sbyte);
    }
}

