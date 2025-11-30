namespace tda26.Server.DTOs.v1;

public class CreateUpdateQuizRequest {
    public Guid? Uuid { get; set; }
    public string Title { get; set; } = null!;
    public int AttemptsCount { get; set; }

    public List<CreateUpdateQuestionRequest> Questions { get; set; } = new();
}