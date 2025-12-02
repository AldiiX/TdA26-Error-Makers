using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data.Models;

[Index(nameof(Username), IsUnique = true)]
public class Account : Auditable {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    [NotMapped]
    public string FullName => Username;

    [NotMapped]
    public string FullNameWithoutTitles => Username;

    [MaxLength(512), JsonIgnore]
    public string Password { get; set; } = string.Empty;
    
    [JsonIgnore]
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [NotMapped]
    public IEnumerable<Like> Likes => Ratings.OfType<Like>();

    [NotMapped]
    public IEnumerable<Dislike> Dislikes => Ratings.OfType<Dislike>();
}