using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

public class CourseModule : Auditable {

    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Required, MaxLength(128)]
    public required string Title { get; set; }

    [MaxLength(1048)]
    public string? Description { get; set; }

    public bool IsVisible { get; set; } = false;

    public int Order { get; set; } = 0;

    [Required]
    public Guid CourseUuid { get; set; }

    [ForeignKey(nameof(CourseUuid)), JsonIgnore]
    public Course Course { get; set; } = null!;

    public ICollection<Material> Materials { get; set; } = new List<Material>();

    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
