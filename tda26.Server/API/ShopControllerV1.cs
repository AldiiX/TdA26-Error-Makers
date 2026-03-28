using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

	[HttpPost("shop/{uuid}/buy")]
	public async Task<IActionResult> BuyShopItem(Guid uuid, CancellationToken ct = default) {
		var loggedAccount = await auth.ReAuthAsync(ct);
		if (loggedAccount == null) return new UnauthorizedObjectResult(new { message = "Nejste přihlášen." });

		var account = await db.AccountsEf().FirstOrDefaultAsync(a => a.Uuid == loggedAccount.Uuid, ct);
		if (account == null) return new UnauthorizedObjectResult(new { message = "Účet nenalezen." });

		var item = await db.ShopItems.FindAsync(new object[] { uuid }, ct);
		if (item == null) return new NotFoundObjectResult(new { message = "Položka nenalezena." });

		if (account.ShopItems.Any(i => i.Uuid == uuid))
			return new ConflictObjectResult(new { message = "Tuto položku již vlastníte." });

		if (account.Ducks < item.PriceInDucks)
			return new UnprocessableEntityObjectResult(new { message = "Nedostatek kačen." });

		account.Ducks -= item.PriceInDucks;
		account.ShopItems.Add(item);
		await db.SaveChangesAsync(ct);

		return new OkObjectResult(new { message = "Položka zakoupena.", ducks = account.Ducks });
	}

	[HttpGet("me/inventory")]
	public async Task<IActionResult> GetInventory(CancellationToken ct = default) {
		var loggedAccount = await auth.ReAuthAsync(ct);
		if (loggedAccount == null) return new UnauthorizedObjectResult(new { message = "Nejste přihlášen." });

		var account = await db.AccountsEf().FirstOrDefaultAsync(a => a.Uuid == loggedAccount.Uuid, ct);
		if (account == null) return new UnauthorizedObjectResult(new { message = "Účet nenalezen." });

		var items = account.ShopItems.Select(i => i.ToDto()).ToList();
		return new JsonResult(items);
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