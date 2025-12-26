using System.Text.Json;
using System.Text.Json.Serialization;

namespace tda26.Server.DTOs.Converters;

public sealed class LowerCaseJsonNamingPolicy : JsonNamingPolicy {
	public override string ConvertName(string name) {
		return name.ToLowerInvariant();
	}
}

public sealed class JsonStringEnumLowerCaseConverter(bool allowIntegerValues = true) : JsonStringEnumConverter(new LowerCaseJsonNamingPolicy(), allowIntegerValues);