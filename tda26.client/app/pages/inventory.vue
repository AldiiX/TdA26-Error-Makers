<script setup lang="ts">
import { ClientOnly } from '#components';
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
import type { Account, ShopItem } from "#shared/types";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>('loggedAccount');
        if (!user.value) return navigateTo('/login');
    }
});

// SEO
useSeo({
    title: "Inventář",
});

const loggedAccount = useState<Account | null>('loggedAccount', () => null);

const { data: items, pending } = useAsyncData("inventory-items", async () => {
    const res = await $fetch<ShopItem[]>("/api/v1/me/inventory");
    return res;
}, { server: false });

function typeLabel(type: ShopItem["type"]): string {
    const labels: Record<ShopItem["type"], string> = {
        avatar: "Avatar",
        banner: "Banner",
        effect: "Efekt",
        badge: "Odznak",
        title: "Titul",
    };
    return labels[type] ?? type;
}

function typeColor(type: ShopItem["type"]): string {
    const colors: Record<ShopItem["type"], string> = {
        avatar: "var(--accent-color-primary)",
        banner: "var(--accent-color-secondary-theme)",
        effect: "#9c5cf6",
        badge: "#e05c1a",
        title: "#0fa3b1",
    };
    return colors[type] ?? "var(--accent-color-primary)";
}
</script>

<template>
    <Teleport to="#teleports">
        <CircleBlurBlob top="12.5vh" right="-12.5vw" color="var(--accent-color-primary)" size="20vw" blur="14vw" />
        <CircleBlurBlob top="100vh" left="-12.5vw" color="var(--accent-color-secondary-theme)" size="20vw" blur="14vw" />
    </Teleport>

    <section>
        <div :class="$style.header">
            <h1 :class="$style.nadpis">Inventář</h1>
            <ClientOnly>
                <div v-if="loggedAccount" :class="$style.balance">
                    <img src="/icons/coin.svg" alt="kačeny" :class="$style.coinIcon" />
                    <span>{{ loggedAccount.ducks }} kačen</span>
                </div>
            </ClientOnly>
        </div>
        <p :class="$style.podnapis">Zde najdete všechny položky, které jste zakoupili v obchodě.</p>

        <!-- Loading skeleton -->
        <div v-if="pending" :class="$style.grid">
            <div v-for="i in 4" :key="i" :class="[$style.card, $style.cardSkeleton]" />
        </div>

        <!-- Items grid -->
        <div v-else-if="items && items.length > 0" :class="$style.grid">
            <div
                v-for="item in items"
                :key="item.uuid"
                :class="$style.card"
            >
                <div :class="$style.cardImage" :style="{ '--type-color': typeColor(item.type) }">
                    <img v-if="item.imageUrl" :src="item.imageUrl" :alt="item.name" />
                    <div v-else :class="$style.cardImagePlaceholder">
                        <span>{{ typeLabel(item.type)[0] }}</span>
                    </div>
                </div>

                <div :class="$style.cardBody">
                    <span :class="$style.typeBadge" :style="{ '--type-color': typeColor(item.type) }">
                        {{ typeLabel(item.type) }}
                    </span>

                    <h3 :class="$style.itemName">{{ item.name }}</h3>
                    <p :class="$style.itemDescription">{{ item.description }}</p>

                    <div :class="$style.ownedBadge">
                        <span>✓ Vlastníte</span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Empty state -->
        <div v-else :class="$style.empty">
            <div :class="$style.emptyIcon">🛍️</div>
            <h3>Inventář je prázdný</h3>
            <p>Zatím jste si nic nezakoupili.</p>
            <NuxtLink to="/shop" :class="$style.shopLink">Přejít do obchodu</NuxtLink>
        </div>
    </section>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.header {
    display: flex;
    align-items: center;
    gap: 20px;
    flex-wrap: wrap;
}

.nadpis {
    font-size: 64px;
    margin: 0;
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
    margin-bottom: 0;
}

.balance {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 10px 18px;
    border-radius: 50px;
    background: rgb(from var(--background-color-secondary) r g b / 0.7);
    border: 1px solid rgb(from var(--text-color-primary) r g b / 0.1);
    backdrop-filter: blur(8px);
    font-weight: 700;
    font-size: 16px;
    color: var(--accent-color-primary);

    .coinIcon {
        width: 20px;
        height: 20px;
    }
}

.grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
    gap: 24px;
    margin-top: 32px;
    margin-bottom: 48px;
    animation: fadeIn 0.6s forwards ease;

    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(20px); }
        to   { opacity: 1; transform: translateY(0); }
    }
}

.card {
    display: flex;
    flex-direction: column;
    border-radius: 20px;
    overflow: hidden;
    background: rgb(from var(--background-color-secondary) r g b / 0.6);
    border: 1px solid rgb(from var(--text-color-primary) r g b / 0.08);
    backdrop-filter: blur(8px);
    transition: transform 0.25s ease, box-shadow 0.25s ease;

    &:hover {
        transform: translateY(-4px);
        box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
    }
}

.cardSkeleton {
    height: 340px;
    background: linear-gradient(
        90deg,
        rgb(from var(--background-color-secondary) r g b / 0.4) 25%,
        rgb(from var(--background-color-secondary) r g b / 0.7) 50%,
        rgb(from var(--background-color-secondary) r g b / 0.4) 75%
    );
    background-size: 200% 100%;
    animation: shimmer 1.5s infinite;

    @keyframes shimmer {
        0%   { background-position: 200% 0; }
        100% { background-position: -200% 0; }
    }
}

.cardImage {
    width: 100%;
    aspect-ratio: 16/9;
    background: linear-gradient(135deg, rgb(from var(--type-color) r g b / 0.15), rgb(from var(--type-color) r g b / 0.35));
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;

    img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }
}

.cardImagePlaceholder {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: rgb(from var(--type-color) r g b / 0.3);
    border: 2px solid rgb(from var(--type-color) r g b / 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 28px;
    font-weight: 900;
    color: var(--type-color);
}

.cardBody {
    display: flex;
    flex-direction: column;
    gap: 10px;
    padding: 18px;
    flex: 1;
}

.typeBadge {
    display: inline-block;
    padding: 3px 10px;
    border-radius: 50px;
    font-size: 12px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    background: rgb(from var(--type-color) r g b / 0.15);
    color: var(--type-color);
    border: 1px solid rgb(from var(--type-color) r g b / 0.3);
    width: fit-content;
}

.itemName {
    margin: 0;
    font-size: 18px;
    font-weight: 700;
    color: var(--text-color-primary);
    line-height: 1.3;
}

.itemDescription {
    margin: 0;
    font-size: 14px;
    color: var(--text-color-secondary);
    line-height: 1.5;
    flex: 1;
}

.ownedBadge {
    padding: 9px 16px;
    border-radius: 10px;
    background: rgb(from var(--accent-color-secondary-theme) r g b / 0.15);
    color: var(--accent-color-secondary-theme);
    border: 1px solid rgb(from var(--accent-color-secondary-theme) r g b / 0.3);
    font-size: 14px;
    font-weight: 700;
    width: fit-content;
}

.empty {
    text-align: center;
    padding: 80px 0;
    color: var(--text-color-secondary);
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;
    animation: fadeIn 0.6s forwards ease;

    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(20px); }
        to   { opacity: 1; transform: translateY(0); }
    }

    .emptyIcon {
        font-size: 56px;
        line-height: 1;
    }

    h3 {
        margin: 0;
        font-size: 22px;
        color: var(--text-color-primary);
    }

    p {
        margin: 0;
        font-size: 16px;
    }
}

.shopLink {
    display: inline-block;
    margin-top: 8px;
    padding: 12px 28px;
    border-radius: 12px;
    background: var(--accent-color-primary);
    color: var(--accent-color-primary-text);
    font-weight: 700;
    font-size: 16px;
    text-decoration: none;
    transition: filter 0.2s ease;

    &:hover {
        filter: brightness(0.85);
    }
}

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
    .header {
        justify-content: center;
    }

    .nadpis, .podnapis {
        text-align: center;
        margin: 0 auto;
    }
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
    section {
        margin-top: -50px;
    }

    .grid {
        grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
    }
}

@media screen and (max-width: 500px) {
    .grid {
        grid-template-columns: 1fr;
    }
}
</style>
