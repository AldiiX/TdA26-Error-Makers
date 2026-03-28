<script setup lang="ts">
import { ClientOnly } from "#components";
import type { Account, ShopItem } from "#shared/types";
import formatCzechCount from "#shared/utils/formatCzechCount";
import getBaseUrl from "#shared/utils/getBaseUrl";
import { push } from "notivue";
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
import LoginForm from "~/components/LoginForm.vue";
import Modal from "~/components/Modal.vue";
import RegisterForm from "~/components/RegisterForm.vue";
import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
import TypeWriter from "~/components/TypeWriter.vue";

definePageMeta({
    layout: "normal-page-layout"
});

// SEO
useSeo({
    title: "Obchod"
});

const loggedAccount = useState<Account | null>("loggedAccount", () => null);
const buyingItem = ref<string | null>(null);
const authModalOpen = ref(false);
const authTab = ref<"login" | "register">("login");

const typeLabels: Record<ShopItem["type"], string> = {
    avatar: "Avatar",
    banner: "Banner",
    effect: "Efekt",
    badge: "Odznak",
    title: "Titul"
};

const typeColors: Record<ShopItem["type"], string> = {
    avatar: "var(--accent-color-primary)",
    banner: "var(--accent-color-secondary-theme)",
    effect: "var(--accent-color-additional-1)",
    badge: "var(--accent-color-additional-2)",
    title: "var(--accent-color-additional-4)"
};

const itemTypeKeys = Object.keys(typeLabels) as ShopItem["type"][];

const { data: items, pending } = useAsyncData("shop-items", async () => {
    try {
        const response = await fetch(getBaseUrl() + "/api/v1/shop/");
        if (!response.ok) {
            return [] as ShopItem[];
        }

        return await response.json() as ShopItem[];
    } catch {
        return [] as ShopItem[];
    }
}, {
    server: false,
    default: () => []
});

const allItems = computed(() => items.value ?? []);
const hasShopItems = computed(() => allItems.value.length > 0);

const searchQuery = ref("");
const selectedType = ref<"all" | ShopItem["type"]>("all");
const selectedMaxPrice = ref<number | null>(null);
const selectedSort = ref<"priceAsc" | "priceDesc" | "nameAsc" | "nameDesc">("priceAsc");
const hideOwned = ref(false);

const maxPrice = computed(() => {
    const prices = allItems.value.map((item) => item.priceInDucks);
    return prices.length > 0 ? Math.max(...prices) : 0;
});

const hasActiveFilters = computed(() => {
    return searchQuery.value.trim().length > 0
        || selectedType.value !== "all"
        || selectedMaxPrice.value !== null
        || selectedSort.value !== "priceAsc"
        || hideOwned.value;
});

const filteredItems = computed(() => {
    const lowered = searchQuery.value.trim().toLocaleLowerCase("cs-CZ");

    const filtered = allItems.value.filter((item) => {
        const matchesText = lowered.length === 0
            || item.name.toLocaleLowerCase("cs-CZ").includes(lowered)
            || item.description.toLocaleLowerCase("cs-CZ").includes(lowered);

        const matchesType = selectedType.value === "all" || item.type === selectedType.value;
        const matchesPrice = selectedMaxPrice.value === null || item.priceInDucks <= selectedMaxPrice.value;
        const matchesOwned = !hideOwned.value || !ownsItem(item);

        return matchesText && matchesType && matchesPrice && matchesOwned;
    });

    return filtered.sort((a, b) => {
        if (selectedSort.value === "priceAsc") return a.priceInDucks - b.priceInDucks;
        if (selectedSort.value === "priceDesc") return b.priceInDucks - a.priceInDucks;
        if (selectedSort.value === "nameDesc") return b.name.localeCompare(a.name, "cs-CZ");
        return a.name.localeCompare(b.name, "cs-CZ");
    });
});

function duckWord(count: number): string {
    return formatCzechCount(count, {
        one: "kačenka",
        few: "kačenky",
        many: "kačenek"
    });
}

function ducksLabel(count: number | null | undefined): string {
    const safeCount = typeof count === "number" && Number.isFinite(count) ? count : 0;
    return `${safeCount} ${duckWord(safeCount)}`;
}

function resetFilters() {
    searchQuery.value = "";
    selectedType.value = "all";
    selectedMaxPrice.value = null;
    selectedSort.value = "priceAsc";
    hideOwned.value = false;
}

function typeLabel(type: ShopItem["type"]): string {
    return typeLabels[type] ?? type;
}

function typeColor(type: ShopItem["type"]): string {
    return typeColors[type] ?? "var(--accent-color-primary)";
}

function selectType(type: "all" | ShopItem["type"]) {
    selectedType.value = type;
}

function ownsItem(item: ShopItem): boolean {
    return loggedAccount.value?.shopItems?.some((owned) => owned.uuid === item.uuid) ?? false;
}

function handleAuthSuccess() {
    authModalOpen.value = false;
    window.location.reload();
}

async function buyItem(item: ShopItem) {
    if (!loggedAccount.value) {
        authModalOpen.value = true;
        authTab.value = "login";
        return;
    }

    buyingItem.value = item.uuid;

    try {
        const result = await $fetch<{ message: string; ducks: number }>(`/api/v1/shop/${item.uuid}/buy`, {
            method: "POST"
        });

        loggedAccount.value.ducks = result.ducks;
        loggedAccount.value.shopItems = [...(loggedAccount.value.shopItems ?? []), item];
        push.success({ title: "Zakoupeno", message: `Položka \"${item.name}\" je teď ve tvém inventáři.` });
    } catch (err: any) {
        const message = err?.data?.message ?? "Nepodařilo se zakoupit položku.";
        push.error({ title: "Chyba", message });
    } finally {
        buyingItem.value = null;
    }
}
</script>

<template>
    <Teleport to="#teleports">
        <CircleBlurBlob top="12.5vh" right="-12.5vw" color="var(--accent-color-primary)" size="20vw" blur="14vw" />
        <CircleBlurBlob top="100vh" left="-12.5vw" color="var(--accent-color-secondary-theme)" size="20vw" blur="14vw" />
    </Teleport>

    <section :class="$style.section">
        <div :class="$style.topContainer">
            <div>
                <h1 :class="$style.nadpis">Obchod</h1>

                <ClientOnly>
                    <SmoothSizeWrapper :change-width="false">
                        <TypeWriter
                            text="Vyber si výbavu profilu, odznaky i efekty. Kačenky můžeš získat plněním úkolů v kurzech."
                            :class="$style.podnapis"
                            :start-delay-ms="240"
                        />
                    </SmoothSizeWrapper>
                </ClientOnly>
            </div>

            <ClientOnly>
                <div v-if="loggedAccount" :class="$style.balance">
                    <img src="/icons/duck.svg" alt="kačenky" :class="$style.duckIcon" />
                    <span>{{ ducksLabel(loggedAccount.ducks) }}</span>
                </div>
            </ClientOnly>
        </div>

        <div :class="$style.bottomContainer">
            <div :class="$style.leftColumn">
                <div :class="$style.filtersLeft">
                    <div :class="$style.filtersHeader">
                        <p>Filtry</p>
                        <button v-if="hasActiveFilters" type="button" :class="$style.resetButton" @click="resetFilters">Reset</button>
                    </div>

                    <div :class="$style.searchBar">
                        <div :class="$style.searchIcon" />
                        <input v-model="searchQuery" type="text" placeholder="Hledat položku...">
                    </div>

                    <div :class="$style.filterBlock">
                        <p>Typ</p>
                        <div :class="$style.filterTags">
                            <button
                                type="button"
                                :class="[$style.filterTag, { [$style.activeTag]: selectedType === 'all' }]"
                                @click="selectType('all')"
                            >
                                Vše
                            </button>
                            <button
                                v-for="type in itemTypeKeys"
                                :key="type"
                                type="button"
                                :class="[$style.filterTag, { [$style.activeTag]: selectedType === type }]"
                                @click="selectType(type)"
                            >
                                {{ typeLabel(type) }}
                            </button>
                        </div>
                    </div>

                    <div :class="$style.filterBlock">
                        <p>Cena</p>
                        <div :class="$style.filterTags">
                            <button
                                type="button"
                                :class="[$style.filterTag, { [$style.activeTag]: selectedMaxPrice === null }]"
                                @click="selectedMaxPrice = null"
                            >
                                Bez limitu
                            </button>
                            <button
                                v-for="limit in [250, 500, 800, 1200]"
                                :key="limit"
                                type="button"
                                :class="[$style.filterTag, { [$style.activeTag]: selectedMaxPrice === limit }]"
                                @click="selectedMaxPrice = limit"
                            >
                                Do {{ limit }}
                            </button>
                        </div>
                    </div>

                    <div :class="$style.filterBlock">
                        <label :class="$style.switchLabel">
                            <input v-model="hideOwned" type="checkbox">
                            <span>Skrýt již vlastněné položky</span>
                        </label>
                        <p :class="$style.helperText">Nejdražší položka: {{ ducksLabel(maxPrice) }}</p>
                    </div>
                </div>
            </div>

            <div :class="$style.rightColumn">
                <div :class="$style.filtersTop">
                    <p>Seřadit:</p>
                    <div :class="$style.sortOptionsList">
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'priceAsc'" @click="selectedSort = 'priceAsc'">Cena od nejnižší</button>
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'priceDesc'" @click="selectedSort = 'priceDesc'">Cena od nejvyšší</button>
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'nameAsc'" @click="selectedSort = 'nameAsc'">Název A-Z</button>
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'nameDesc'" @click="selectedSort = 'nameDesc'">Název Z-A</button>
                    </div>
                </div>

                <div v-if="pending" :class="$style.cardsGrid">
                    <div v-for="i in 6" :key="i" :class="[$style.card, $style.cardSkeleton]" />
                </div>

                <p v-else-if="!hasShopItems" :class="$style.emptyState">V obchodě momentálně nejsou žádné položky.</p>

                <div v-else-if="filteredItems.length > 0" :class="$style.cardsGrid">
                    <article
                        v-for="item in filteredItems"
                        :key="item.uuid"
                        :class="$style.card"
                    >
                        <div :class="$style.cardImage" :style="{ '--type-color': typeColor(item.type) }">
                            <img v-if="item.imageUrl" :src="item.imageUrl" :alt="item.name">
                            <div v-else :class="$style.cardImagePlaceholder">
                                <span>{{ typeLabel(item.type).charAt(0) }}</span>
                            </div>
                        </div>

                        <div :class="$style.cardBody">
                            <span :class="$style.typeBadge" :style="{ '--type-color': typeColor(item.type) }">
                                {{ typeLabel(item.type) }}
                            </span>

                            <h3 :class="$style.itemName">{{ item.name }}</h3>
                            <p :class="$style.itemDescription">{{ item.description }}</p>

                            <div :class="$style.cardFooter">
                                <div :class="$style.price">
                                    <img src="/icons/duck.svg" alt="kačenky" :class="$style.duckIconSmall" />
                                    <span>{{ item.priceInDucks }}</span>
                                </div>

                                <button
                                    v-if="!ownsItem(item)"
                                    :class="[$style.buyButton, {
                                        [$style.disabledButton]: loggedAccount && loggedAccount.ducks < item.priceInDucks,
                                        [$style.loadingButton]: buyingItem === item.uuid
                                    }]"
                                    :disabled="buyingItem === item.uuid || (!!loggedAccount && loggedAccount.ducks < item.priceInDucks)"
                                    @click="buyItem(item)"
                                >
                                    <span v-if="buyingItem === item.uuid" :class="$style.spinner" />
                                    <span v-else-if="loggedAccount && loggedAccount.ducks < item.priceInDucks">Nedostatek kačenek</span>
                                    <span v-else-if="!loggedAccount">Přihlásit se</span>
                                    <span v-else>Koupit</span>
                                </button>

                                <div v-else :class="$style.ownedBadge">✓ Vlastníte</div>
                            </div>
                        </div>
                    </article>
                </div>

                <p v-else :class="$style.emptyState">Podle zvolených filtrů jsme nenašli žádné položky.</p>
            </div>
        </div>

        <Modal
            :enabled="authModalOpen"
            can-be-closed-by-clicking-outside
            :modal-style="{ maxWidth: '500px' }"
            @close="authModalOpen = false"
        >
            <SmoothSizeWrapper>
                <div :class="$style.authModalHeader">
                    <h3>Pro nákup v obchodě se musíš přihlásit.</h3>

                    <div :class="$style.authTabs">
                        <button
                            :class="[$style.authTab, authTab === 'login' ? $style.authTabActive : '']"
                            type="button"
                            @click="authTab = 'login'"
                        >
                            Přihlášení
                        </button>

                        <button
                            :class="[$style.authTab, authTab === 'register' ? $style.authTabActive : '']"
                            type="button"
                            @click="authTab = 'register'"
                        >
                            Registrace
                        </button>
                    </div>
                </div>

                <div :class="$style.authFormContainer">
                    <Transition
                        mode="out-in"
                        :enter-active-class="$style.fadeEnterActive"
                        :enter-from-class="$style.fadeEnterFrom"
                        :enter-to-class="$style.fadeEnterTo"
                        :leave-active-class="$style.fadeLeaveActive"
                        :leave-from-class="$style.fadeLeaveFrom"
                        :leave-to-class="$style.fadeLeaveTo"
                    >
                        <div :key="authTab">
                            <LoginForm
                                v-if="authTab === 'login'"
                                @login-success="handleAuthSuccess"
                            />
                            <RegisterForm
                                v-else
                                @register-success="handleAuthSuccess"
                            />
                        </div>
                    </Transition>
                </div>
            </SmoothSizeWrapper>
        </Modal>
    </section>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.liquidGlass {
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 4px 30px rgba(0, 0, 0, 0.15);
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 0.6);
    backdrop-filter: blur(8px) saturate(1.6);
}

.section {
    display: flex;
    flex-direction: column;
    gap: 28px;
}

.topContainer {
    display: flex;
    justify-content: space-between;
    align-items: flex-end;
    gap: 24px;
    flex-wrap: wrap;
}

.nadpis {
    margin: 0;
    font-size: 64px;
}

.podnapis {
    margin-top: 16px;
    max-width: 760px;
    height: 48px;
    color: var(--text-color-secondary);
    font-size: 20px;
}

.balance {
    display: inline-flex;
    align-items: center;
    gap: 10px;
    border-radius: 999px;
    padding: 10px 18px;
    font-weight: 700;
    color: var(--accent-color-primary);
    background: rgb(from var(--background-color-secondary) r g b / 0.65);
    border: 1px solid rgb(from var(--text-color-primary) r g b / 0.1);
    backdrop-filter: blur(8px);
}

.duckIcon {
    width: 20px;
    height: 20px;
}

.bottomContainer {
    display: flex;
    width: 100%;
    min-height: 72vh;
    gap: 48px;
    align-items: flex-start;
}

.leftColumn {
    display: flex;
    min-width: 348px;
    width: 22%;
    border-radius: 24px;
    padding: 32px;

    @extend .liquidGlass;
}

.rightColumn {
    display: flex;
    flex-direction: column;
    gap: 24px;
    min-width: 0;
    flex: 1 1 auto;
}

.filtersLeft {
    width: 100%;
    display: flex;
    flex-direction: column;
}

.filtersHeader {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;

    p {
        margin: 0;
        font-size: 36px;
        font-weight: 700;
    }
}

.resetButton {
    all: unset;
    cursor: pointer;
    color: var(--text-color-primary);
    font-weight: 700;

    &:hover {
        opacity: 0.6;
    }
}

.searchBar {
    width: 100%;
    background-color: var(--background-color-3);
    display: flex;
    align-items: center;
    gap: 10px;
    border-radius: 12px;
    padding: 12px 16px;

    input {
        width: 100%;
        border: none;
        outline: none;
        font-size: 18px;
        color: var(--text-color-secondary);
        background: transparent;
        font-family: Dosis, sans-serif;

        &::placeholder {
            color: var(--text-color-secondary);
            opacity: 0.8;
        }
    }
}

.searchIcon {
    width: 24px;
    height: 24px;
    background-color: var(--text-color-secondary);
    opacity: 0.8;
    mask-image: url("/icons/search.svg");
    mask-size: cover;
    mask-position: center;
    mask-repeat: no-repeat;
}

.filterBlock {
    margin-top: 24px;

    > p {
        font-size: 20px;
        font-weight: 600;
        margin: 0;
        margin-bottom: 10px;
    }
}

.filterTags {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
}

.filterTag {
    background-color: var(--background-color-3);
    border: none;
    padding: 8px 16px;
    border-radius: 999px;
    margin: 0;
    font-size: 14px;
    cursor: pointer;
    user-select: none;
    transition-duration: 0.3s;
    color: var(--text-color-primary);
    font-family: Dosis, sans-serif;

    &:hover {
        background: var(--background-color-primary);
    }
}

.activeTag {
    background: var(--accent-color-primary);
    color: var(--accent-color-primary-text);
}

.switchLabel {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    font-size: 15px;
}

.helperText {
    margin: 10px 0 0;
    color: var(--text-color-secondary);
    font-size: 14px;
}

.filtersTop {
    display: flex;
    align-items: center;
    gap: 16px 48px;
    padding: 20px;
    min-height: 64px;
    width: 100%;
    border-radius: 24px;
    flex-wrap: wrap;

    @extend .liquidGlass;

    > p {
        font-size: 18px;
        color: var(--text-color-secondary);
        margin: 0;
        margin-left: 4px;
        font-weight: 600;
    }
}

.sortOptionsList {
    display: flex;
    gap: 12px;
    align-items: center;
    flex-wrap: wrap;
}

.sortOption {
    appearance: none;
    border: none;
    background: transparent;
    padding: 8px 12px;
    border-radius: 10px;
    font-size: 16px;
    color: var(--text-color-secondary);
    cursor: pointer;
    transition-duration: 0.3s;
    user-select: none;
    font-family: Dosis, sans-serif;

    &:hover {
        background-color: var(--accent-color-secondary-theme);
        color: var(--accent-color-secondary-theme-text);
    }

    &[data-active="true"] {
        background-color: var(--accent-color-primary);
        color: var(--accent-color-primary-text);
    }
}

.cardsGrid {
    margin-top: 4px;
    display: grid;
    gap: 32px;
    grid-template-columns: repeat(auto-fill, minmax(348px, 1fr));
    align-items: stretch;
}

.card {
    display: flex;
    flex-direction: column;
    border-radius: 24px;
    overflow: hidden;
    min-height: 380px;
    box-shadow: 0 0 32px rgba(0, 0, 0, 0.1);

    @extend .liquidGlass;

    transition: transform 0.25s ease, box-shadow 0.25s ease;

    &:hover {
        transform: translateY(-4px);
        box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 12px 34px rgba(0, 0, 0, 0.2);
    }
}

.cardSkeleton {
    background: linear-gradient(
        90deg,
        rgb(from var(--background-color-secondary) r g b / 0.4) 25%,
        rgb(from var(--background-color-secondary) r g b / 0.7) 50%,
        rgb(from var(--background-color-secondary) r g b / 0.4) 75%
    );
    background-size: 200% 100%;
    animation: shimmer 1.5s infinite;
}

@keyframes shimmer {
    0% { background-position: 200% 0; }
    100% { background-position: -200% 0; }
}

.cardImage {
    width: 100%;
    aspect-ratio: 16 / 9;
    background: linear-gradient(135deg, rgb(from var(--type-color) r g b / 0.2), rgb(from var(--type-color) r g b / 0.36));
    display: flex;
    align-items: center;
    justify-content: center;

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
    border: 2px solid rgb(from var(--type-color) r g b / 0.5);
    background: rgb(from var(--type-color) r g b / 0.3);
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: Dosis, sans-serif;

    span {
        font-size: 26px;
        font-weight: 900;
        color: var(--type-color);
    }
}

.cardBody {
    display: flex;
    flex-direction: column;
    gap: 10px;
    padding: 16px;
    flex: 1;
}

.typeBadge {
    width: fit-content;
    padding: 4px 10px;
    border-radius: 999px;
    font-size: 12px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.35px;
    color: var(--type-color);
    border: 1px solid rgb(from var(--type-color) r g b / 0.33);
    background: rgb(from var(--type-color) r g b / 0.15);
}

.itemName {
    margin: 0;
    font-size: 22px;
    line-height: 1.2;
}

.itemDescription {
    margin: 0;
    color: var(--text-color-secondary);
    font-size: 15px;
    line-height: 1.5;
    flex: 1;
}

.cardFooter {
    margin-top: 6px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
}

.price {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    color: var(--accent-color-primary);
    font-weight: 800;
    font-size: 20px;
    font-family: Dosis, sans-serif;
}

.duckIconSmall {
    width: 18px;
    height: 18px;
}

.buyButton {
    border: none;
    border-radius: 10px;
    padding: 9px 14px;
    min-width: 110px;
    cursor: pointer;
    background: var(--accent-color-primary);
    color: var(--accent-color-primary-text);
    font-size: 13px;
    font-weight: 700;
    font-family: Dosis, sans-serif;
    display: flex;
    align-items: center;
    justify-content: center;

    &:not(:disabled):hover {
        filter: brightness(0.9);
    }
}

.disabledButton {
    background: rgb(from var(--text-color-primary) r g b / 0.12);
    color: var(--text-color-secondary);
    cursor: not-allowed;
}

.loadingButton {
    pointer-events: none;
}

.spinner {
    width: 14px;
    height: 14px;
    border-radius: 50%;
    border: 2px solid currentColor;
    border-top-color: transparent;
    animation: spin 0.8s linear infinite;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}

.ownedBadge {
    border-radius: 10px;
    padding: 9px 14px;
    color: var(--accent-color-secondary-theme);
    border: 1px solid rgb(from var(--accent-color-secondary-theme) r g b / 0.35);
    background: rgb(from var(--accent-color-secondary-theme) r g b / 0.15);
    font-size: 13px;
    font-weight: 700;
}

.emptyState {
    margin-top: 12px;
    color: var(--text-color-secondary);
    font-size: 16px;
}

.authModalHeader {
    margin-bottom: 24px;

    h3 {
        margin: 0 0 24px;
    }
}

.authTabs {
    display: flex;
    gap: 8px;
    border-bottom: 2px solid var(--background-color-3);
}

.authTab {
    flex: 1;
    padding: 12px 16px;
    background: transparent;
    border: none;
    border-bottom: 2px solid transparent;
    color: var(--text-color-secondary);
    font-size: 16px;
    font-weight: 600;
    border-radius: 24px 24px 0 0;
    cursor: pointer;
    margin-bottom: -2px;

    &:hover {
        color: var(--text-color-primary);
        background: var(--background-color-3);
    }
}

.authTabActive {
    color: var(--accent-color-primary);
    border-bottom-color: var(--accent-color-primary);
}

.authFormContainer {
    margin-top: 24px;
}

.fadeEnterActive {
    transition: 300ms ease;
}

.fadeLeaveActive {
    transition: 200ms ease;
}

.fadeEnterFrom,
.fadeLeaveTo {
    opacity: 0;
}

.fadeEnterTo,
.fadeLeaveFrom {
    opacity: 1;
}

@media screen and (max-width: app.$laptopBreakpoint) {
    .topContainer {
        justify-content: center;
    }

    .nadpis,
    .podnapis {
        text-align: center;
        margin-left: auto;
        margin-right: auto;
    }

    .balance {
        margin: 0 auto;
    }

    .bottomContainer {
        flex-direction: column;
        gap: 24px;
    }

    .leftColumn {
        width: 100%;
        min-width: 0;
    }
}

@media screen and (max-width: app.$tabletBreakpoint) {
    .section {
        margin-top: -50px;
    }

    .cardsGrid {
        grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
        gap: 20px;
    }
}

@media screen and (max-width: 760px) {
    .podnapis {
        height: 76px;
    }
}

@media screen and (max-width: 500px) {
    .podnapis {
        height: 104px;
    }

    .cardsGrid {
        grid-template-columns: 1fr;
    }
}
</style>

