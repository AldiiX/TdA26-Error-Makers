using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

[Table("Quizzes")]
public class Quiz : Auditable {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    public Guid CourseUuid { get; set; }

    [ForeignKey("CourseUuid")]
    public Course Course { get; set; } = null!;
}