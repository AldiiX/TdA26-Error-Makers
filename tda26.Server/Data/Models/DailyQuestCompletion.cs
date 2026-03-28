using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
namespace tda26.Server.Data.Models;

public enum DailyQuestType {
    CompleteQuiz = 0,
    AccessMaterial = 1
}

[Table("DailyQuestCompletions")]
[Index(nameof(AccountUuid), nameof(Date), nameof(QuestType), IsUnique = true)]
public class DailyQuestCompletion {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public Guid AccountUuid { get; set; }
    [ForeignKey(nameof(AccountUuid)), JsonIgnore]
    public Account Account { get; set; } = null!;
    public DailyQuestType QuestType { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}