using System.Text.Json;
using System.Text.Json.Serialization;

namespace tda26.Server.DTOs.Converters;

/// <summary>
/// JSON converter that ensures DateTime values are always serialized as UTC with 'Z' suffix.
/// This fixes timezone issues where dates without timezone info are interpreted as local time by JavaScript.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTime = reader.GetDateTime();
        
        // If the DateTime doesn't have a Kind specified, assume it's UTC
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
        
        // If it's Local, convert to UTC
        if (dateTime.Kind == DateTimeKind.Local)
        {
            return dateTime.ToUniversalTime();
        }
        
        return dateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Ensure the DateTime is treated as UTC
        DateTime utcDateTime = value.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => value
        };
        
        // Write in ISO 8601 format with 'Z' suffix
        writer.WriteStringValue(utcDateTime);
    }
}
