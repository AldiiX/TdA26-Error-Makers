using System.Text.Json;
using System.Text.Json.Serialization;

namespace tda26.Server.DTOs.Converters;

public sealed class CamelCaseJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name)
        => JsonNamingPolicy.CamelCase.ConvertName(name);
}

public sealed class JsonStringEnumCamelCaseConverter : JsonStringEnumConverter
{
    public JsonStringEnumCamelCaseConverter()
        : base(new CamelCaseJsonNamingPolicy(), allowIntegerValues: true)
    {
    }

    public JsonStringEnumCamelCaseConverter(bool allowIntegerValues)
        : base(new CamelCaseJsonNamingPolicy(), allowIntegerValues)
    {
    }
}