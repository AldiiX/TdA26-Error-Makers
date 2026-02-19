namespace tda26.Server.DTOs.v1;

public class CreateUpdateSingleChoiceQuestionRequest : CreateUpdateQuestionRequest {
    public int CorrectIndex { get; set; }
}
