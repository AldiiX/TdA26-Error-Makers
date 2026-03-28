namespace tda26.Server.Data.Models;

public class BannerShopItem : ShopItem, IShopItemWithImageUrl {
	public required string ImageUrl { get; set; }
}