using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data.Models;

[Table("Tags"), Index(nameof(DisplayName))]
public class Tag : Auditable {
    
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(64)] public required string DisplayName { get; set; }

    [JsonIgnore]
    public Guid? CategoryUuid { get; set; }

    [ForeignKey(nameof(CategoryUuid))]
    public Category? Category { get; set; }
    
    [JsonIgnore]
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}