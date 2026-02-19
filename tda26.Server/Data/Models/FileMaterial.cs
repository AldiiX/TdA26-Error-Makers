using System.ComponentModel.DataAnnotations;

namespace tda26.Server.Data.Models;

public sealed class FileMaterial : Material {
    [Required] [MaxLength(256)] public required string FileUrl { get; set; }
    [MaxLength(256)] public string? MimeType { get; set; }
    public int SizeBytes { get; set; }
}