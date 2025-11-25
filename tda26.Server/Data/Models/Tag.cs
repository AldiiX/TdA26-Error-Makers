using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

[Table("Tags")]
public class Tag : IAuditable {
    
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(64)] public required string DisplayName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [JsonIgnore]
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}