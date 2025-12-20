using System.Text.Json;
using System.Text.Json.Serialization;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Converters;

public class QuestionRequestConverter : JsonConverter<CreateUpdateQuestionRequest> {
    public override CreateUpdateQuestionRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("type", out var typeProp))
            throw new JsonException("Missing 'type'");

        var type = typeProp.GetString();

        return type switch
        {
            "singleChoice" =>
                JsonSerializer.Deserialize<CreateUpdateSingleChoiceQuestionRequest>(root.GetRawText(), options)!,

            "multipleChoice" =>
                JsonSerializer.Deserialize<CreateUpdateMultipleChoiceQuestionRequest>(root.GetRawText(), options)!,

            _ => throw new JsonException($"Unknown question type '{type}'")
        };
    }

    public override void Write(Utf8JsonWriter writer, CreateUpdateQuestionRequest value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}