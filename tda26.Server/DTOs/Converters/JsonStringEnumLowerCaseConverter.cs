using System.Text.Json;
using System.Text.Json.Serialization;

namespace tda26.Server.DTOs.Converters;

public sealed class LowerCaseJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
        => name.ToLowerInvariant();
}

public sealed class JsonStringEnumLowerCaseConverter : JsonStringEnumConverter
{
    public JsonStringEnumLowerCaseConverter()
        : base(new LowerCaseJsonNamingPolicy(), allowIntegerValues: true)
    {
    }

    public JsonStringEnumLowerCaseConverter(bool allowIntegerValues)
        : base(new LowerCaseJsonNamingPolicy(), allowIntegerValues)
    {
    }
}