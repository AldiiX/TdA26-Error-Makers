using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

public sealed class QuestionOption {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid QuestionUuid { get; set; }
    [JsonIgnore, ForeignKey("QuestionUuid")]
    public Question Question { get; set; } = null!;

    [Required] public string Text { get; set; } = string.Empty;
    
    public required int Order { get; set; }
    public bool IsCorrect { get; set; }
}