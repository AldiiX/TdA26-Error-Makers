using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class ReadFileMaterialResponse : ReadMaterialResponse {
    [Required] [MaxLength(256)] public required string FileUrl { get; set; }
    [MaxLength(64)] public string? MimeType { get; set; }
    public int SizeBytes { get; set; }
    
}