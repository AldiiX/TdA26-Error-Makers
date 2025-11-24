using System.ComponentModel.DataAnnotations;

namespace tda26.Server.Data.Models;

public class UrlMaterial : Material {
    [Required] [MaxLength(256)] public required string Url { get; set; }
    [MaxLength(256)] public string? FaviconUrl { get; set; }
}