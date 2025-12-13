using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(FileMaterial), "file")]
[JsonDerivedType(typeof(UrlMaterial), "url")]
public class Material : IAuditable {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();
    
    [Required] [MaxLength(128)] public required string Name { get; set; }
    [MaxLength(1048)] public string? Description { get; set; }
    
    public enum MaterialType {
        File,
        Url
    }

    [Required] public required MaterialType Type { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public Guid CourseUuid { get; set; }
    [ForeignKey("CourseUuid"), JsonIgnore]
    public Course Course { get; set; } = null!;
}