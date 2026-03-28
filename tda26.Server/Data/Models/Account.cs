using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using tda26.Server.DTOs.Converters;

namespace tda26.Server.Data.Models;

[Index(nameof(Username), IsUnique = true)]
public class Account : Auditable {
    // orm props
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(32)]
    public string Username { get; set; } = string.Empty;
    
    [MaxLength(128)]
    public string PrimaryEmail { get; set; } = string.Empty;

    [MaxLength(512), JsonIgnore]
    public string Password { get; set; } = string.Empty;

    public bool IsPremium { get; set; } = false;

    [JsonIgnore]
    public Guid? OrganizationUuid { get; set; }

    [ForeignKey(nameof(OrganizationUuid))]
    public Organization? Organization { get; set; }

    [JsonIgnore]
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

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