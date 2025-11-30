using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public class QuizAnswerOption {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid AnswerUuid { get; set; }
    [ForeignKey("AnswerUuid")]
    public QuizAnswer Answer { get; set; } = null!;

    public Guid OptionUuid { get; set; }
    [ForeignKey("OptionUuid")]
    public QuestionOption Option { get; set; } = null!;
}