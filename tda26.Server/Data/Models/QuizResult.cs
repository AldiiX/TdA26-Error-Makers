using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public class QuizResult {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid QuizUuid { get; set; }
    [ForeignKey("QuizUuid")]
    public Quiz Quiz { get; set; } = null!;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public int Score { get; set; }

    public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
}