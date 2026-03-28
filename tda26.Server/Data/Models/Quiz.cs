using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using tda26.Server.DTOs.Converters;

namespace tda26.Server.Data.Models;

[JsonConverter(typeof(JsonStringEnumLowerCaseConverter))]
public enum QuizMode {
    Practice = 0,
    FinalTest = 1
}

[Table("Quizzes")]
public class Quiz : Auditable, IModule {

    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [Required, MaxLength(128)]
    public string Title { get; set; } = string.Empty;
    
    public int AttemptsCount { get; set; }

    public Guid CourseUuid { get; set; }

    [ForeignKey("CourseUuid"), JsonIgnore]
    public Course Course { get; set; } = null!;

    [JsonIgnore]
    public Guid? ModuleUuid { get; set; }

    [ForeignKey(nameof(ModuleUuid)), JsonIgnore]
    public CourseModule? Module { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    
    public bool IsVisible { get; set; } = false;

    public int Order { get; set; } = 0;

    public QuizMode Mode { get; set; } = QuizMode.Practice;
}