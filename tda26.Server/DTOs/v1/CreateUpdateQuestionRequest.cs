namespace tda26.Server.DTOs.v1;

public abstract class CreateUpdateQuestionRequest {
    public Guid? Uuid { get; set; }
    public string Type { get; set; } = null!;
    public string Question { get; set; } = null!;
    public List<string> Options { get; set; } = new();
    public int Order { get; set; }
}
