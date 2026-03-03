using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class ReadUrlMaterialResponse : ReadMaterialResponse {
    [Required] [MaxLength(256)] public required string Url { get; set; }
    [MaxLength(256)] public string? FaviconUrl { get; set; }
}
