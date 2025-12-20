namespace tda26.Server.DTOs.v1;

public class ReadMultipleChoiceQuestionResponse : ReadQuestionResponse
{
    public List<int> CorrectIndices { get; set; } = new();
}