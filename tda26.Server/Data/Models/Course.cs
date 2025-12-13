using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tda26.Server.Data.Models;

public class Course : Auditable {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1048)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? ImageUrl { get; set; }

    public int ViewCount { get; set; } = 0;

    [NotMapped]
    public int LikeCount => Likes.ToList().Count;

    [JsonIgnore]
    public Guid? LecturerUuid { get; set; }

    [ForeignKey(nameof(LecturerUuid))]
    public Account? Lecturer { get; set; }
    
    public ICollection<Material> Materials { get; set; } = new List<Material>(); 
  
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>(); 
    
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
  
    public ICollection<FeedPost> Feed { get; set; } = new List<FeedPost>();

    [JsonIgnore]
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [NotMapped, JsonIgnore]
    public IEnumerable<Like> Likes => Ratings.OfType<Like>();

    [NotMapped, JsonIgnore]
    public IEnumerable<Dislike> Dislikes => Ratings.OfType<Dislike>();
}