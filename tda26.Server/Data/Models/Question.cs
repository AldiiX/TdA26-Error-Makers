using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public abstract class Question {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();
    
    public Guid QuizUuid { get; set; }
    [ForeignKey("QuizUuid")]
    public Quiz Quiz { get; set; } = null!;

    [Required, MaxLength(1048)] public string Text { get; set; } = string.Empty;

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}