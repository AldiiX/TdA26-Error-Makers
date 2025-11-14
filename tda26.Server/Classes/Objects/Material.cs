using System.Text.Json.Serialization;

namespace tda26.Server.Classes.Objects;

public partial class Material {
    
    public Guid Uuid { get; private set; }
    
    public enum MaterialType {
        Document,
        Image,
        Video,
        Audio
    }
    
    public MaterialType Type { get; private set; }
    
    public string Name { get; private set; }
    
    public string FileUrl { get; private set; }

    public string? Description { get; private set; }
    
    public string? MimeType { get; private set; }
    
    public int SizeBytes { get; private set; }
    
    [JsonConstructor]

    public Material(
        Guid uuid,
        MaterialType type,
        string name,
        string fileUrl,
        string? description,
        string? mimeType,
        int sizeBytes
    )
    {
        Uuid = uuid;
        Type = type;
        Name = name;
        FileUrl = fileUrl;
        Description = description;
        MimeType = mimeType;
        SizeBytes = sizeBytes;
    }
}