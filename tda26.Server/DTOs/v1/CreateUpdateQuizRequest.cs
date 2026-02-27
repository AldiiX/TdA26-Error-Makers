namespace tda26.Server.DTOs.v1;

public class CreateUpdateQuizRequest {
    public Guid? Uuid { get; set; }
    public string Title { get; set; } = null!;
    public int AttemptsCount { get; set; }
    public bool? IsVisible { get; set; }
    public Guid? ModuleUuid { get; set; }

    public List<CreateUpdateQuestionRequest> Questions { get; set; } = new() {
        new CreateUpdateSingleChoiceQuestionRequest {
            Type = "singleChoice",
            Question = "Nová otázka",
            Options = new List<string> { "Možnost 1", "Možnost 2" },
            Order = 0
        }
    };
}
