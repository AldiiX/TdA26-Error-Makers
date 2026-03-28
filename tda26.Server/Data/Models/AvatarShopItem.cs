namespace tda26.Server.Data.Models;

public class AvatarShopItem : ShopItem, IShopItemWithImageUrl {
	public required string ImageUrl { get; set; }
}