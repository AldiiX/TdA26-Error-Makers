using System.ComponentModel.DataAnnotations;

namespace tda26.Server.Data.Models;

public class UrlMaterial : Material {
	[MaxLength(256), Required]
    public required string Url { get; set; }

    [MaxLength(256)]
    public string? FaviconUrl { get; set; }
}