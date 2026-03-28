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

    private static AvatarShopItem Avatar(string name, string description, int price, string seed) => new() {
        Name = name,
        Description = description,
        PriceInDucks = price,
        ImageUrl = $"https://picsum.photos/seed/{seed}/512/512"
    };

    private static BannerShopItem Banner(string name, string description, int price, string seed) => new() {
        Name = name,
        Description = description,
        PriceInDucks = price,
        ImageUrl = $"https://picsum.photos/seed/{seed}/1280/400"
    };

    private static EffectShopItem Effect(string name, string description, int price, string seed) => new() {
        Name = name,
        Description = description,
        PriceInDucks = price,
        ImageUrl = $"https://picsum.photos/seed/{seed}/960/540"
    };

    private static BadgeShopItem Badge(string name, string description, int price, string seed) => new() {
        Name = name,
        Description = description,
        PriceInDucks = price,
        ImageUrl = $"https://picsum.photos/seed/{seed}/512/512"
    };

    private static TitleShopItem Title(string name, string description, int price) => new() {
        Name = name,
        Description = description,
        PriceInDucks = price
    };

    private static List<ShopItem> BuildSeedItems() {
        return [
            // Avatar (6)
            Avatar("Neonovy profil", "Avatar s jemnym neonovym glow efektem.", 180, "avatar-neon"),
            Avatar("Aqua fokus", "Cisty avatar ve stylu brand barev.", 220, "avatar-aqua"),
            Avatar("Polar glow", "Studeny avatar s decentnim prechodem.", 260, "avatar-polar"),
            Avatar("Night pulse", "Tmavy avatar se sviticim okrajem.", 300, "avatar-night"),
            Avatar("Mint wave", "Lehky avatar v sekundarni brand palete.", 340, "avatar-mint"),
            Avatar("Core pixel", "Minimalisticky avatar se ctvercovym motivem.", 380, "avatar-pixel"),

            // Banner (6)
            Banner("Wave banner", "Siroky profilovy banner s wave motivem.", 360, "banner-wave"),
            Banner("Blue grid", "Minimalisticky banner s kontrastnim prechodem.", 520, "banner-grid"),
            Banner("Liquid glass", "Skleneny banner inspirovany UI stylizaci.", 560, "banner-glass"),
            Banner("Hex stream", "Technicky banner se sitovym vzorem.", 620, "banner-hex"),
            Banner("Night runway", "Tmavy banner s neony a hloubkou.", 700, "banner-runway"),
            Banner("Signal stripe", "Dynamicky banner pro aktivni profily.", 760, "banner-signal"),

            // Effect (6)
            Effect("Pulse efekt", "Animovany efekt zvyraznujici aktivniho uzivatele.", 480, "effect-pulse"),
            Effect("Soft aura", "Jemna aura kolem profiloveho elementu.", 540, "effect-aura"),
            Effect("Spark ring", "Kruhovy efekt s kratkymi odlesky.", 620, "effect-spark"),
            Effect("Focus rays", "Liniovy efekt s modernim rasterem.", 700, "effect-rays"),
            Effect("Echo blur", "Dvojity stinovy efekt s plynulym dozvukem.", 760, "effect-echo"),
            Effect("Flux core", "Vyrazny premium efekt s hlubsim kontrastem.", 860, "effect-flux"),

            // Badge (5)
            Badge("Top resitel", "Odznak za stabilni vysledky v kvizech.", 300, "badge-top"),
            Badge("Sprint badge", "Odznak pro rychle dokonceni studia.", 360, "badge-sprint"),
            Badge("Kviz maestro", "Odznak pro vysoke skore napric kvizy.", 430, "badge-maestro"),
            Badge("Daily streak", "Odznak za pravidelnou aktivitu.", 500, "badge-streak"),
            Badge("Team helper", "Odznak za pomoc ostatnim studentum.", 580, "badge-helper"),

            // Title (5)
            Title("Mistr kurzu", "Exkluzivni titul k profilovemu jmenu.", 900),
            Title("Kodovy navigator", "Titul pro uzivatele s orientaci v kodu.", 980),
            Title("Debug veteran", "Titul pro trpelive a peclive ladeni.", 1120),
            Title("Challenge hunter", "Titul pro fanousky narocnych ukolu.", 1260),
            Title("Academy legend", "Vyberovy titul pro top profily.", 1500)
        ];
    }
}
