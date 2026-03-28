using tda26.Server.Data.Models;

namespace tda26.Server.DTOs;

public class ShopItemDto {
	public required Guid Uuid { get; set; }
	public required string Name { get; set; }
	public required string Description { get; set; }
	public required int PriceInDucks { get; set; }
	public required string Type  { get; set; }
	public string? ImageUrl { get; set; }
}