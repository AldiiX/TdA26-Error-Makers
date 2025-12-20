using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

public class QuizAnswer {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid QuizResultUuid { get; set; }
    [JsonIgnore, ForeignKey("QuizResultUuid")]
    public QuizResult QuizResult { get; set; } = null!;

    public Guid QuestionUuid { get; set; }
    [ForeignKey("QuestionUuid")]
    public Question Question { get; set; } = null!;

    public ICollection<QuizAnswerOption> SelectedOptions { get; set; } = new List<QuizAnswerOption>();
}