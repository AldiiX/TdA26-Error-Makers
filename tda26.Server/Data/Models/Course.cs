using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

public class Course : IAuditable {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1048)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [MaxLength(512)]
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public Guid? LecturerUuid { get; set; }

    [ForeignKey(nameof(LecturerUuid))]
    public Lecturer? Lecturer { get; set; } = null!;
    
    public ICollection<Material> Materials { get; set; } = new List<Material>(); 
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>(); 
    public ICollection<FeedPost> Feed { get; set; } = new List<FeedPost>();
}