using Microsoft.EntityFrameworkCore;
using tda26.Server.Data.Models;

namespace tda26.Server.Data;

public static class ShopItemSeeder {
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default) {
        var existingItems = await db.ShopItems.AsNoTracking().ToListAsync(ct);
        var existingKeys = existingItems
            .Select(i => BuildKey(i.GetType().Name, i.Name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seedItems = BuildSeedItems();
        var itemsToInsert = seedItems
            .Where(i => !existingKeys.Contains(BuildKey(i.GetType().Name, i.Name)))
            .ToList();

        if (itemsToInsert.Count == 0) {
            return;
        }

        await db.ShopItems.AddRangeAsync(itemsToInsert, ct);
        await db.SaveChangesAsync(ct);
    }

    private static string BuildKey(string typeName, string name) => $"{typeName}|{name}";

    private static List<ShopItem> BuildSeedItems() {
        return [
            // Avatar
            new AvatarShopItem { Name = "Neonovy profil", Description = "Avatar s jemnym neonovym glow efektem.", PriceInDucks = 180, ImageUrl = string.Empty },
            new AvatarShopItem { Name = "Aqua fokus", Description = "Cisty avatar ve stylu brand barev.", PriceInDucks = 220, ImageUrl = string.Empty },
            new AvatarShopItem { Name = "Polar glow", Description = "Studeny avatar s decentnim prechodem.", PriceInDucks = 260, ImageUrl = string.Empty },
            new AvatarShopItem { Name = "Night pulse", Description = "Tmavy avatar se sviticim okrajem.", PriceInDucks = 300, ImageUrl = string.Empty },
            new AvatarShopItem { Name = "Mint wave", Description = "Lehky avatar v sekundarni brand palete.", PriceInDucks = 340, ImageUrl = string.Empty },
            new AvatarShopItem { Name = "Core pixel", Description = "Minimalisticky avatar se ctvercovym motivem.", PriceInDucks = 380, ImageUrl = string.Empty },

            // Banner
            new BannerShopItem { Name = "Wave banner", Description = "Siroky profilovy banner s wave motivem.", PriceInDucks = 360, ImageUrl = string.Empty },
            new BannerShopItem { Name = "Blue grid", Description = "Minimalisticky banner s kontrastnim prechodem.", PriceInDucks = 520, ImageUrl = string.Empty },
            new BannerShopItem { Name = "Liquid glass", Description = "Skleneny banner inspirovany UI stylizaci.", PriceInDucks = 560, ImageUrl = string.Empty },
            new BannerShopItem { Name = "Hex stream", Description = "Technicky banner se sitovym vzorem.", PriceInDucks = 620, ImageUrl = string.Empty },
            new BannerShopItem { Name = "Night runway", Description = "Tmavy banner s neony a hloubkou.", PriceInDucks = 700, ImageUrl = string.Empty },
            new BannerShopItem { Name = "Signal stripe", Description = "Dynamicky banner pro aktivni profily.", PriceInDucks = 760, ImageUrl = string.Empty },

            // Effect
            new EffectShopItem { Name = "Pulse efekt", Description = "Animovany efekt zvyraznujici aktivniho uzivatele.", PriceInDucks = 480, ImageUrl = string.Empty },
            new EffectShopItem { Name = "Soft aura", Description = "Jemna aura kolem profiloveho elementu.", PriceInDucks = 540, ImageUrl = string.Empty },
            new EffectShopItem { Name = "Spark ring", Description = "Kruhovy efekt s kratkymi odlesky.", PriceInDucks = 620, ImageUrl = string.Empty },
            new EffectShopItem { Name = "Focus rays", Description = "Liniovy efekt s modernim rasterem.", PriceInDucks = 700, ImageUrl = string.Empty },
            new EffectShopItem { Name = "Echo blur", Description = "Dvojity stinovy efekt s plynulym dozvukem.", PriceInDucks = 760, ImageUrl = string.Empty },
            new EffectShopItem { Name = "Flux core", Description = "Vyrazny premium efekt s hlubsim kontrastem.", PriceInDucks = 860, ImageUrl = string.Empty },

            // Badge
            new BadgeShopItem { Name = "Top resitel", Description = "Odznak za stabilni vysledky v kvizech.", PriceInDucks = 300, ImageUrl = string.Empty },
            new BadgeShopItem { Name = "Sprint badge", Description = "Odznak pro rychle dokonceni studia.", PriceInDucks = 360, ImageUrl = string.Empty },
            new BadgeShopItem { Name = "Kviz maestro", Description = "Odznak pro vysoke skore napric kvizy.", PriceInDucks = 430, ImageUrl = string.Empty },
            new BadgeShopItem { Name = "Daily streak", Description = "Odznak za pravidelnou aktivitu.", PriceInDucks = 500, ImageUrl = string.Empty },
            new BadgeShopItem { Name = "Team helper", Description = "Odznak za pomoc ostatnim studentum.", PriceInDucks = 580, ImageUrl = string.Empty },

            // Title
            new TitleShopItem { Name = "Mistr kurzu", Description = "Exkluzivni titul k profilovemu jmenu.", PriceInDucks = 900 },
            new TitleShopItem { Name = "Kodovy navigator", Description = "Titul pro uzivatele s orientaci v kodu.", PriceInDucks = 980 },
            new TitleShopItem { Name = "Debug veteran", Description = "Titul pro trpelive a peclive ladeni.", PriceInDucks = 1120 },
            new TitleShopItem { Name = "Challenge hunter", Description = "Titul pro fanousky narocnych ukolu.", PriceInDucks = 1260 },
            new TitleShopItem { Name = "Academy legend", Description = "Vyberovy titul pro top profily.", PriceInDucks = 1500 }
        ];
    }
}

