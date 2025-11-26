using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

public class Course : IAuditable {

    [Table("Likes")]
    public class Like : IAuditable {
        [Key] public Guid Uuid { get; set; } = Guid.NewGuid();
        public Guid AccountUuid { get; set; }

        [ForeignKey("AccountUuid")]
        public Account Account { get; set; } = null!;

        public Guid CourseUuid { get; set; }

        [ForeignKey("CourseUuid"), JsonIgnore]
        public Course Course { get; set; } = null!;



        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }


    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();
    [MaxLength(128)] public string Name { get; set; } = string.Empty;
    [MaxLength(1048)] public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    [MaxLength(512)] public string? ImageUrl { get; set; }
    public int Views { get; set; } = 0;

    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Material> Materials { get; set; } = new List<Material>(); 
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>(); 
    public ICollection<FeedPost> Feed { get; set; } = new List<FeedPost>(); 
}