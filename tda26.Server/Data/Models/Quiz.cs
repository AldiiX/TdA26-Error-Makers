using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

[Table("Quizzes")]
public class Quiz : IAuditable {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();
    [MaxLength(128)] public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public Guid CourseUuid { get; set; }

    [ForeignKey("CourseUuid")]
    public Course Course { get; set; } = null!;
}