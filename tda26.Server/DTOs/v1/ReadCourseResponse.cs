namespace tda26.Server.DTOs.v1;

public class ReadCourseResponse {
    public required Guid Uuid { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public Guid? LecturerUuid { get; set; }
    public string? ImageUrl { get; set; }
    public required ICollection<ReadMaterialResponse> Materials { get; set; }
    public required ICollection<ReadQuizResponse> Quizzes { get; set; }
    public required ICollection<ReadFeedResponse> Feed { get; set; }
}