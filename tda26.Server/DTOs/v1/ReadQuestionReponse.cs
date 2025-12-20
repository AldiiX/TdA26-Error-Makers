namespace tda26.Server.DTOs.v1;

public abstract class ReadQuestionResponse
{
    public Guid Uuid { get; set; }
    public string Type { get; set; } = default!;
    public string Question { get; set; } = default!;
    public List<string> Options { get; set; } = new();
    public List<int>? SelectedIndices { get; set; }
    public bool? IsCorrect { get; set; }
}