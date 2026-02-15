using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class ReadCourseResponse {
    public required Guid Uuid { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
    public required CourseStatus Status { get; set; }
    public required int ViewCount { get; set; }
    public required int LikeCount { get; set; }
    public string? ImageUrl { get; set; }
    public required string CategoryImageUrl { get; set; }
    public Lecturer? Lecturer { get; set; } = null!;
    public Account? Account { get; set; } = null!;
    public AuthorDto? Author { get; set; }
    public required ICollection<ReadMaterialResponse> Materials { get; set; }
    public required ICollection<ReadQuizResponse> Quizzes { get; set; }
    public required ICollection<FeedPost> Feed { get; set; }
    public required byte RatingScore { get; set; }
    public Category? Category { get; set; }
    public List<Tag>? Tags { get; set; }
}