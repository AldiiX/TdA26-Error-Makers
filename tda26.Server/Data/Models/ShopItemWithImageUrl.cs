namespace tda26.Server.Data.Models;

public class ShopItemWithImageUrl : ShopItem, IShopItemWithImageUrl {
	public required string ImageUrl { get; set; }
}