using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

public class Material : Auditable {
    public enum MaterialType {
        File,
        Url
    }



    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();
    
    [Required, MaxLength(128)]
    public required string Name { get; set; }

    [MaxLength(1048)]
    public string? Description { get; set; }

    [Required]
    public required MaterialType Type { get; set; }

    [JsonIgnore, Required]
    public Guid CourseUuid { get; set; }

    [ForeignKey("CourseUuid")]
    public Course Course { get; set; } = null!;
}