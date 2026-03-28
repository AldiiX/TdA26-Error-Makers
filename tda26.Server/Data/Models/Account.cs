using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using tda26.Server.DTOs.Converters;

namespace tda26.Server.Data.Models;

[Index(nameof(Username), IsUnique = true)]
public class Account : Auditable {
    public const int BaseXpPerLevel = 100;
    public const int XpGrowthPerLevel = 25;

    // orm props
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(32)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(128)]
    public string PrimaryEmail { get; set; } = string.Empty;

    [MaxLength(512), JsonIgnore]
    public string Password { get; set; } = string.Empty;

	public ICollection<ShopItem> ShopItems { get; set; } = new List<ShopItem>();

    public Guid? EquippedAvatarUuid { get; set; }
    public Guid? EquippedBannerUuid { get; set; }
    public Guid? EquippedEffectUuid { get; set; }
    public Guid? EquippedBadgeUuid { get; set; }
    public Guid? EquippedTitleUuid { get; set; }

    [JsonIgnore]
    public AvatarShopItem? EquippedAvatar { get; set; }

    [JsonIgnore]
    public BannerShopItem? EquippedBanner { get; set; }

    [JsonIgnore]
    public EffectShopItem? EquippedEffect { get; set; }

    [JsonIgnore]
    public BadgeShopItem? EquippedBadge { get; set; }

    [JsonIgnore]
    public TitleShopItem? EquippedTitle { get; set; }
    
    public bool IsPremium { get; set; } = false;

    [JsonIgnore]
    public Guid? OrganizationUuid { get; set; }

    [ForeignKey(nameof(OrganizationUuid))]
    public Organization? Organization { get; set; }

    [JsonIgnore]
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public int Ducks  { get; set; }
    public int Xp { get; set; }
    public int Level { get; set; }

    public void AddProgress(int xpDelta, int ducksDelta) {
        if (xpDelta > 0) {
            Xp += xpDelta;
            RecalculateLevelFromXp();
        }

        if (ducksDelta > 0) {
            Ducks += ducksDelta;
        }
    }

    public void RecalculateLevelFromXp() {
        var remainingXp = Math.Max(0, Xp);
        var level = 0;

        while (remainingXp >= GetXpRequiredForNextLevel(level)) {
            remainingXp -= GetXpRequiredForNextLevel(level);
            level++;
        }

        Level = level;
    }

    private static int GetXpRequiredForNextLevel(int currentLevel) {
        return BaseXpPerLevel + (currentLevel * XpGrowthPerLevel);
    }



    // nemapovany props (pouze pro serializaci)
    [JsonConverter(typeof(JsonStringEnumLowerCaseConverter))]
    public enum AccountType { Account, Lecturer, Admin, Student }

    [NotMapped]
    public IEnumerable<Like> Likes => Ratings.OfType<Like>();

    [NotMapped]
    public IEnumerable<Dislike> Dislikes => Ratings.OfType<Dislike>();

    [NotMapped]
    public string FullName => Username;

    [NotMapped]
    public string FullNameWithoutTitles => Username;

    [NotMapped]
    public AccountType Type => AccountType.Account;

}