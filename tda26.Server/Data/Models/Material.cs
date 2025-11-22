using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public class Material : IAuditable {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();

    public enum MaterialType {
        Document,
        Image,
        Video,
        Audio
    }

    public MaterialType Type { get; set; }
    [MaxLength(128)] public string Name { get; set; } = string.Empty;
    [MaxLength(256)] public string FileUrl { get; set; } = string.Empty;
    [MaxLength(1048)] public string? Description { get; set; }
    [MaxLength(64)] public string? MimeType { get; set; }
    public int SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public Guid CourseUuid { get; set; }
    [ForeignKey("CourseUuid")]
    public Course Course { get; set; } = null!;
}