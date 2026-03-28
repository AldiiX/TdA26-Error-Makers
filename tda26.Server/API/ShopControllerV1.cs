using Microsoft.AspNetCore.Mvc;
using tda26.Server.Data;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.Mapping;
using tda26.Server.Infrastructure;
using tda26.Server.Services;

namespace tda26.Server.API;

[ApiController]
[Route("api/v1")]
public class ShopControllerV1(AppDbContext db, IAuthService auth) {

	[HttpGet("shop")]
	public IActionResult GetShopItems() {
		var si = db.ShopItems.ToList().Select(i => i.ToDto()).ToList();

		return new JsonResult(si);
	}

	[HttpGet("shop/g")]
	public async Task<IActionResult> GenerateRandomShopItems() {
		var loggedAccount = await auth.ReAuthAsync();
		if (loggedAccount == null) return new UnauthorizedResult();

		var entity = db.AccountsEf().FirstOrDefault(a => a.Uuid == loggedAccount.Uuid);
		if (entity == null) return new UnauthorizedResult();

		entity.ShopItems.Add(new AvatarShopItem() {
			Name = "Cool avatar",
			Description = "A really cool avatar for your profile.",
			PriceInDucks = 100,
			ImageUrl = "https://example.com/cool-avatar.png"
		});

		await db.SaveChangesAsync();
		return new OkResult();
	}
}