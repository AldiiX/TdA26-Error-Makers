using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

[Table("Quizzes")]
public class Quiz : Auditable {

    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Required, MaxLength(128)]
    public string Title { get; set; } = string.Empty;
    
    public int AttemptsCount { get; set; }

    public Guid CourseUuid { get; set; }

    [ForeignKey("CourseUuid"), JsonIgnore]
    public Course Course { get; set; } = null!;
    
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}