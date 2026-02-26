namespace tda26.Server.DTOs.v1;

public class ReadModuleResponse {
    public required Guid Uuid { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required bool IsVisible { get; set; }
    public required int Order { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public required DateTimeOffset UpdatedAt { get; set; }
    public required ICollection<ReadMaterialResponse> Materials { get; set; }
    public required ICollection<ReadQuizResponse> Quizzes { get; set; }
}
