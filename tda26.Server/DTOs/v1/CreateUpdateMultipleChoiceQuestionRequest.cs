namespace tda26.Server.DTOs.v1;

public class CreateUpdateMultipleChoiceQuestionRequest : CreateUpdateQuestionRequest {
    public List<int> CorrectIndices { get; set; } = new();
}
