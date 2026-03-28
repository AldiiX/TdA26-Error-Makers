using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.Mapping;

public static class ShopItemMapper {
	public static ShopItemDto ToDto(this ShopItem item) {
		return new ShopItemDto() {
			Uuid = item.Uuid,
			Name = item.Name,
			Description = item.Description,
			PriceInDucks = item.PriceInDucks,
			Type = item switch {
				AvatarShopItem => "avatar",
				BannerShopItem => "banner",
				EffectShopItem => "effect",
				BadgeShopItem => "badge",
				TitleShopItem => "title",
				_ => throw new ArgumentException("Unknown shop item type")
			},
			ImageUrl = (item as IShopItemWithImageUrl)?.ImageUrl
		};
	}
}