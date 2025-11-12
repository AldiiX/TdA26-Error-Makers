using System.Text.Json.Serialization;

namespace tda26.Server.Classes.Objects;

public partial class Course {
    public Guid Uuid { get; private set; } 
    
    public string Name { get; private set; }
    
    public string Description { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    
    public DateTime UpdatedAt { get; private set; }

    public string? ImageUrl { get; private set; }

    public List<Material> Materials { get; private set; } = [];

    public List<Quiz> Quizzes { get; private set; } = [];

    public List<FeedPost> Feed { get; private set; } = [];



    [JsonConstructor]
    public Course(
        Guid uuid,
        string name,
        string description,
        DateTime createdAt,
        DateTime updatedAt,
        string? imageUrl 
    )
    {
        Uuid = uuid;
        Name = name;
        Description = description;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        ImageUrl = imageUrl;
    }
    
}