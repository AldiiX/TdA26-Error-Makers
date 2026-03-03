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
    public required DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTimeOffset UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsVisible { get; set; }
    public int Order { get; set; } = 0;
}
