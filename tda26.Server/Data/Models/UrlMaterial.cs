using System.ComponentModel.DataAnnotations;

namespace tda26.Server.Data.Models;

public sealed class UrlMaterial : Material {
	[MaxLength(256), Required]
    public required string Url { get; set; }

    [MaxLength(256)]
    public string? FaviconUrl { get; set; }
    
    public DateTime? LastClickedAt { get; set; }
    
    public int ClickCount { get; set; }
    
}