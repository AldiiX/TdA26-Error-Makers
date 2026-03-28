using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data.Models;

[Index(nameof(DailyRewardDayUuid), nameof(TaskCode), IsUnique = true)]
[Table(name: "DailyRewardTaskStates")]
public sealed class DailyRewardTaskState : Auditable {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid DailyRewardDayUuid { get; set; }

    [ForeignKey(nameof(DailyRewardDayUuid)), JsonIgnore]
    public DailyRewardDay DailyRewardDay { get; set; } = null!;

    [MaxLength(64)]
    public string TaskCode { get; set; } = string.Empty;

    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Description { get; set; } = string.Empty;

    public int CurrentValue { get; set; }

    public int TargetValue { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}

