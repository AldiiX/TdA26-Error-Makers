using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace tda26.Server.Data.Models;

public class Course : Auditable {

    // mapovani props na sloupce v db
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1048)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? ImageUrl { get; set; }

    public int ViewCount { get; set; } = 0;

    [JsonIgnore]
    public Guid? LecturerUuid { get; set; }

    [ForeignKey(nameof(LecturerUuid))]
    public Account? Account { get; set; } = null!;

    [JsonIgnore]
    public Guid? CategoryUuid { get; set; }

    [ForeignKey(nameof(CategoryUuid))]
    public Category? Category { get; set; } = null!;
    
    public ICollection<Material> Materials { get; set; } = new List<Material>(); 
  
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>(); 
    
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
  
    public ICollection<FeedPost> Feed { get; set; } = new List<FeedPost>();

    [JsonIgnore]
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();




    // ciste c# nemapovane propy
    [NotMapped, JsonIgnore]
    public IEnumerable<Like> Likes => Ratings.OfType<Like>();

    [NotMapped, JsonIgnore]
    public IEnumerable<Dislike> Dislikes => Ratings.OfType<Dislike>();

    [NotMapped]
    public int LikeCount => Likes.ToList().Count;

    [NotMapped]
    public string ImageUrlOrDefault => string.IsNullOrEmpty(ImageUrl) ? (Category?.Icon ?? "/icons/courseicons/question.svg") : ImageUrl;

    [NotMapped]
    public byte RatingScore {
        get {
            // vypocet score od 0 do 10 na zaklade pomeru like/dislikes + TODO: recenzi (až budou udelany)
            var likeCount = LikeCount;
            var dislikeCount = Dislikes.ToList().Count;
            var totalCount = likeCount + dislikeCount;
            if (totalCount == 0) return 0;
            var score = (double) likeCount / totalCount * 10;
            return (byte) Math.Round(score);
        }
    }

    [NotMapped]
    public Lecturer? Lecturer => Account as Lecturer;
}