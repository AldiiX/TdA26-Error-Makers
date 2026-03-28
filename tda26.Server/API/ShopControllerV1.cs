using Microsoft.AspNetCore.Mvc;
using tda26.Server.Data;
using tda26.Server.Data.Models;
using tda26.Server.DTOs.Mapping;
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

		if (loggedAccount is not Student student) return new BadRequestResult();

		db.ShopItems.Add(new BannerShopItem() {
			Name = "Test Banner Item",
			Description = "This is a test item.",
			PriceInDucks = 100,
			ImageUrl = "https://placehold.co/192x108"
		});

		await db.SaveChangesAsync();
		return new OkObjectResult(loggedAccount);
	}
}