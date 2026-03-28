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
        var shopItems = db.ShopItems
            .AsNoTracking()
            .ToList()
            .Select(i => i.ToDto())
            .ToList();

        return new JsonResult(shopItems);
    }

    [HttpPost("shop/{uuid:guid}/buy")]
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

        return new OkObjectResult(new {
            message = "Položka zakoupena.",
            ducks = account.Ducks
        });
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

    [HttpGet("me/profile")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct = default) {
        var loggedAccount = await auth.ReAuthAsync(ct);
        if (loggedAccount == null) return new UnauthorizedObjectResult(new { message = "Nejste přihlášen." });

        var account = await db.AccountsEf().FirstOrDefaultAsync(a => a.Uuid == loggedAccount.Uuid, ct);
        if (account == null) return new UnauthorizedObjectResult(new { message = "Účet nenalezen." });

        return new JsonResult(BuildProfileResponse(account));
    }

    [HttpPost("me/inventory/{uuid:guid}/equip")]
    public async Task<IActionResult> EquipInventoryItem(Guid uuid, CancellationToken ct = default) {
        var loggedAccount = await auth.ReAuthAsync(ct);
        if (loggedAccount == null) return new UnauthorizedObjectResult(new { message = "Nejste přihlášen." });

        var account = await db.AccountsEf().FirstOrDefaultAsync(a => a.Uuid == loggedAccount.Uuid, ct);
        if (account == null) return new UnauthorizedObjectResult(new { message = "Účet nenalezen." });

        var ownedItem = account.ShopItems.FirstOrDefault(i => i.Uuid == uuid);
        if (ownedItem == null) return new NotFoundObjectResult(new { message = "Položka není ve vašem inventáři." });

        switch (ownedItem) {
            case AvatarShopItem avatar:
                account.EquippedAvatarUuid = avatar.Uuid;
                account.EquippedAvatar = avatar;
                break;
            case BannerShopItem banner:
                account.EquippedBannerUuid = banner.Uuid;
                account.EquippedBanner = banner;
                break;
            case EffectShopItem effect:
                account.EquippedEffectUuid = effect.Uuid;
                account.EquippedEffect = effect;
                break;
            case BadgeShopItem badge:
                account.EquippedBadgeUuid = badge.Uuid;
                account.EquippedBadge = badge;
                break;
            case TitleShopItem title:
                account.EquippedTitleUuid = title.Uuid;
                account.EquippedTitle = title;
                break;
            default:
                return new BadRequestObjectResult(new { message = "Nepodporovaný typ položky." });
        }

        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new {
            message = "Položka byla vybavena.",
            profile = BuildProfileResponse(account)
        });
    }

    [HttpPost("me/inventory/{type}/unequip")]
    public async Task<IActionResult> UnequipInventoryItem(string type, CancellationToken ct = default) {
        var loggedAccount = await auth.ReAuthAsync(ct);
        if (loggedAccount == null) return new UnauthorizedObjectResult(new { message = "Nejste přihlášen." });

        var account = await db.AccountsEf().FirstOrDefaultAsync(a => a.Uuid == loggedAccount.Uuid, ct);
        if (account == null) return new UnauthorizedObjectResult(new { message = "Účet nenalezen." });

        switch (type.Trim().ToLowerInvariant()) {
            case "avatar":
                account.EquippedAvatarUuid = null;
                account.EquippedAvatar = null;
                break;
            case "banner":
                account.EquippedBannerUuid = null;
                account.EquippedBanner = null;
                break;
            case "effect":
                account.EquippedEffectUuid = null;
                account.EquippedEffect = null;
                break;
            case "badge":
                account.EquippedBadgeUuid = null;
                account.EquippedBadge = null;
                break;
            case "title":
                account.EquippedTitleUuid = null;
                account.EquippedTitle = null;
                break;
            default:
                return new BadRequestObjectResult(new { message = "Neznámý slot pro odebrání vybavení." });
        }

        await db.SaveChangesAsync(ct);
        return new OkObjectResult(new {
            message = "Položka byla odebrána z vybavení.",
            profile = BuildProfileResponse(account)
        });
    }

    private static object BuildProfileResponse(Account account) {
        var inventory = account.ShopItems.Select(i => i.ToDto()).ToList();

        return new {
            account = new {
                account.Uuid,
                account.Username,
                account.FullName,
                account.FullNameWithoutTitles,
                type = account.Type.ToString().ToLowerInvariant(),
                account.Ducks,
                account.Xp,
                account.Level
            },
            equipped = new {
                avatar = account.EquippedAvatar?.ToDto(),
                banner = account.EquippedBanner?.ToDto(),
                effect = account.EquippedEffect?.ToDto(),
                badge = account.EquippedBadge?.ToDto(),
                title = account.EquippedTitle?.ToDto()
            },
            inventory
        };
    }
}