namespace tda26.Server.DTOs.v1;

public class ReadSingleChoiceQuestionResponse : ReadQuestionResponse
{
    public int CorrectIndex { get; set; }
}