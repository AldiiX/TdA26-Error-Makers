using System.Text.Json.Serialization;

namespace tda26.Server.Classes.Objects;

public class Quiz {
    
    public Guid Uuid { get; private set; }
    
    public string Title { get; private set; }

    public int QuestionCount { get; private set; }

    [JsonConstructor]
    
    public Quiz(
        Guid uuid,
        string title,
        int questionCount
    )
    {
        Uuid = uuid;
        Title = title;
        QuestionCount = questionCount;
    }
}