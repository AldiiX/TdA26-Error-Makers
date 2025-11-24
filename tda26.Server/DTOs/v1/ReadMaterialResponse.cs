using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tda26.Server.DTOs.v1;

[JsonPolymorphic]
[JsonDerivedType(typeof(ReadUrlMaterialResponse))]
[JsonDerivedType(typeof(ReadFileMaterialResponse))]
public class ReadMaterialResponse {
    [Required] public required Guid Uuid { get; set; }
    [Required] [MaxLength(128)] public required string Name { get; set; }
    [MaxLength(1048)] public string? Description { get; set; }
    [Required] public required string Type { get; set; }
    public required DateTime CreatedAt { get; set; } = DateTime.Now;
    public required DateTime UpdatedAt { get; set; } = DateTime.Now;
}