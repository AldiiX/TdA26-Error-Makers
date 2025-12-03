namespace tda26.Server.DTOs.v1;

public class CreateQuizSubmissionRequest {
    public List<AnswerSubmission> Answers { get; set; }
}

public class AnswerSubmission {
    public Guid Uuid { get; set; }
    public int? SelectedIndex { get; set; }
    public List<int>? SelectedIndices { get; set; }
}