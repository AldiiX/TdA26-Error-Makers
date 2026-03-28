using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data.Models;

[Index(nameof(AccountUuid), nameof(RewardDate), IsUnique = true)]
[Table(name: "DailyRewardDays")]
public sealed class DailyRewardDay : Auditable {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid AccountUuid { get; set; }

    [ForeignKey(nameof(AccountUuid)), JsonIgnore]
    public Account Account { get; set; } = null!;

    public DateOnly RewardDate { get; set; }

    public DateTimeOffset? ClaimedAt { get; set; }

    [JsonIgnore]
    public ICollection<DailyRewardTaskState> Tasks { get; set; } = new List<DailyRewardTaskState>();
}

