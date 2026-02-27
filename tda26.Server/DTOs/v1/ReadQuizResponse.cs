namespace tda26.Server.DTOs.v1;

public class ReadQuizResponse
{
    public Guid Uuid { get; set; }
    public string Title { get; set; } = default!;
    public int AttemptsCount { get; set; }
    
    public DateTimeOffset? CreatedAt { get; set; }

    public List<object> Questions { get; set; } = new();
    
    public bool IsVisible { get; set; }
    public int Order { get; set; } = 0;
}
