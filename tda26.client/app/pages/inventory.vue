<script setup lang="ts">
import { ClientOnly, NuxtLink } from "#components";
import type { Account, ProfilePayload, ShopItem } from "#shared/types";
import formatCzechCount from "#shared/utils/formatCzechCount";
import { push } from "notivue";
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
import LoginForm from "~/components/LoginForm.vue";
import Modal from "~/components/Modal.vue";
import RegisterForm from "~/components/RegisterForm.vue";
import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";

definePageMeta({
    layout: "normal-page-layout"
});

useSeo({
    title: "Inventář"
});

const loggedAccount = useState<Account | null>("loggedAccount", () => null);
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

const { data: fetchedItems, pending } = useAsyncData("inventory-items", async () => {
    if (!loggedAccount.value) {
        return [] as ShopItem[];
    }

    try {
        return await $fetch<ShopItem[]>("/api/v1/me/inventory");
    } catch {
        return [] as ShopItem[];
    }
}, {
    server: false,
    default: () => []
});

const { data: profileData, refresh: refreshProfile } = useAsyncData("me-profile-inventory", async () => {
    if (!loggedAccount.value) return null;

    try {
        return await $fetch<ProfilePayload>("/api/v1/me/profile");
    } catch {
        return null;
    }
}, {
    server: false,
    default: () => null
});

const equipLoadingUuid = ref<string | null>(null);

onMounted(() => {
    if (!loggedAccount.value) {
        authModalOpen.value = true;
        authTab.value = "login";
    }
});

const allItems = computed(() => fetchedItems.value ?? []);
const hasInventoryItems = computed(() => allItems.value.length > 0);

const searchQuery = ref("");
const selectedType = ref<"all" | ShopItem["type"]>("all");
const selectedSort = ref<"nameAsc" | "nameDesc" | "priceAsc" | "priceDesc">("nameAsc");

const hasActiveFilters = computed(() => {
    return searchQuery.value.trim().length > 0
        || selectedType.value !== "all"
        || selectedSort.value !== "nameAsc";
});

const filteredItems = computed(() => {
    const lowered = searchQuery.value.trim().toLocaleLowerCase("cs-CZ");

    const filtered = allItems.value.filter((item) => {
        const matchesText = lowered.length === 0
            || item.name.toLocaleLowerCase("cs-CZ").includes(lowered)
            || item.description.toLocaleLowerCase("cs-CZ").includes(lowered);

        const matchesType = selectedType.value === "all" || item.type === selectedType.value;
        return matchesText && matchesType;
    });

    return filtered.sort((a, b) => {
        if (selectedSort.value === "nameDesc") return b.name.localeCompare(a.name, "cs-CZ");
        if (selectedSort.value === "priceAsc") return a.priceInDucks - b.priceInDucks;
        if (selectedSort.value === "priceDesc") return b.priceInDucks - a.priceInDucks;
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

function ducksLabel(count: number): string {
    return `${count} ${duckWord(count)}`;
}

function resetFilters() {
    searchQuery.value = "";
    selectedType.value = "all";
    selectedSort.value = "nameAsc";
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

function equippedItemByType(type: ShopItem["type"]): ShopItem | null {
    const equipped = profileData.value?.equipped;
    if (!equipped) return null;

    if (type === "avatar") return equipped.avatar;
    if (type === "banner") return equipped.banner;
    if (type === "effect") return equipped.effect;
    if (type === "badge") return equipped.badge;
    return equipped.title;
}

function isEquipped(item: ShopItem): boolean {
    return equippedItemByType(item.type)?.uuid === item.uuid;
}

async function equipItem(item: ShopItem) {
    if (!loggedAccount.value) {
        authModalOpen.value = true;
        return;
    }

    equipLoadingUuid.value = item.uuid;

    try {
        await $fetch(`/api/v1/me/inventory/${item.uuid}/equip`, { method: "POST" });
        await refreshProfile();
        push.success({ title: "Vybaveno", message: `Položka \"${item.name}\" je aktivní.` });
    } catch (err: any) {
        push.error({ title: "Chyba", message: err?.data?.message ?? "Nepodařilo se vybavit položku." });
    } finally {
        equipLoadingUuid.value = null;
    }
}

async function unequipItem(type: ShopItem["type"]) {
    if (!loggedAccount.value) {
        authModalOpen.value = true;
        return;
    }

    equipLoadingUuid.value = `slot-${type}`;

    try {
        await $fetch(`/api/v1/me/inventory/${type}/unequip`, { method: "POST" });
        await refreshProfile();
        push.success({ title: "Odebrano", message: "Položka byla odebrána z vybavení." });
    } catch (err: any) {
        push.error({ title: "Chyba", message: err?.data?.message ?? "Nepodařilo se odebrat vybavení." });
    } finally {
        equipLoadingUuid.value = null;
    }
}

function handleAuthSuccess() {
    authModalOpen.value = false;
    window.location.reload();
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
                <h1 :class="$style.nadpis">Inventář</h1>
                <p :class="$style.podnapis">Všechny zakoupené položky na jednom místě, ve stejném vizuálním stylu jako katalog kurzů.</p>
            </div>

            <ClientOnly>
                <div v-if="loggedAccount" :class="$style.balance">
                    <img src="/icons/duck.svg" alt="kačenky" :class="$style.duckIcon" />
                    <span>{{ ducksLabel(loggedAccount.ducks) }}</span>
                </div>
            </ClientOnly>
        </div>

        <div v-if="!loggedAccount" :class="$style.guestNotice">
            Pro zobrazení inventáře se přihlas.
            <button type="button" :class="$style.inlineAction" @click="authModalOpen = true">Přihlásit se</button>
        </div>

        <div v-else :class="$style.bottomContainer">
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
                </div>
            </div>

            <div :class="$style.rightColumn">
                <div :class="$style.filtersTop">
                    <p>Seřadit:</p>
                    <div :class="$style.sortOptionsList">
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'nameAsc'" @click="selectedSort = 'nameAsc'">Název A-Z</button>
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'nameDesc'" @click="selectedSort = 'nameDesc'">Název Z-A</button>
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'priceAsc'" @click="selectedSort = 'priceAsc'">Cena od nejnižší</button>
                        <button type="button" :class="$style.sortOption" :data-active="selectedSort === 'priceDesc'" @click="selectedSort = 'priceDesc'">Cena od nejvyšší</button>
                    </div>
                </div>

                <div v-if="pending" :class="$style.cardsGrid">
                    <div v-for="i in 4" :key="i" :class="[$style.card, $style.cardSkeleton]" />
                </div>

                <div v-else-if="!hasInventoryItems" :class="$style.emptyState">
                    <p>Inventář je zatím prázdný.</p>
                    <NuxtLink to="/shop" :class="$style.shopLink">Přejít do obchodu</NuxtLink>
                </div>

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

                                <div :class="$style.footerActions">
                                    <div v-if="isEquipped(item)" :class="$style.equippedBadge">✓ Vybaveno</div>

                                    <button
                                        v-if="!isEquipped(item)"
                                        type="button"
                                        :class="$style.equipButton"
                                        :disabled="equipLoadingUuid === item.uuid"
                                        @click="equipItem(item)"
                                    >
                                        <span v-if="equipLoadingUuid === item.uuid" :class="$style.spinner" />
                                        <span v-else>Equip</span>
                                    </button>

                                    <button
                                        v-else
                                        type="button"
                                        :class="[$style.equipButton, $style.equipButtonSecondary]"
                                        :disabled="equipLoadingUuid === `slot-${item.type}`"
                                        @click="unequipItem(item.type)"
                                    >
                                        <span v-if="equipLoadingUuid === `slot-${item.type}`" :class="$style.spinner" />
                                        <span v-else>Odebrat</span>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </article>
                </div>

                <div v-else :class="$style.emptyState">
                    <p>Podle zvolených filtrů jsme nenašli žádné položky.</p>
                    <NuxtLink to="/shop" :class="$style.shopLink">Přejít do obchodu</NuxtLink>
                </div>
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
                    <h3>Pro zobrazení inventáře se musíš přihlásit.</h3>

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
    gap: 24px;
}

.topContainer {
    display: flex;
    justify-content: space-between;
    align-items: flex-end;
    gap: 16px;
    flex-wrap: wrap;
}

.nadpis {
    margin: 0;
    font-size: 64px;
}

.podnapis {
    margin-top: 16px;
    margin-bottom: 0;
    max-width: 760px;
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

.guestNotice {
    width: fit-content;
    border-radius: 12px;
    padding: 10px 14px;
    font-size: 14px;
    font-weight: 600;
    color: var(--accent-color-primary);
    background: rgb(from var(--accent-color-primary) r g b / 0.13);
    border: 1px solid rgb(from var(--accent-color-primary) r g b / 0.28);
}

.inlineAction {
    margin-left: 10px;
    border: none;
    background: transparent;
    color: inherit;
    font-size: 14px;
    font-weight: 800;
    cursor: pointer;
    border-bottom: 1px solid currentColor;
    font-family: Dosis, sans-serif;
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
    min-height: 360px;
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

.footerActions {
    display: flex;
    align-items: center;
    gap: 8px;
}

.price {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    color: var(--accent-color-primary);
    font-weight: 800;
    font-size: 18px;
}

.duckIconSmall {
    width: 18px;
    height: 18px;
}

.equippedBadge {
    border-radius: 10px;
    padding: 9px 14px;
    color: var(--accent-color-secondary-theme);
    border: 1px solid rgb(from var(--accent-color-secondary-theme) r g b / 0.35);
    background: rgb(from var(--accent-color-secondary-theme) r g b / 0.15);
    font-size: 13px;
    font-weight: 700;
}

.equipButton {
    border: none;
    border-radius: 10px;
    padding: 9px 14px;
    min-width: 92px;
    cursor: pointer;
    background: var(--accent-color-primary);
    color: var(--accent-color-primary-text);
    font-size: 13px;
    font-weight: 700;
    font-family: Dosis, sans-serif;
    display: inline-flex;
    align-items: center;
    justify-content: center;

    &:hover:not(:disabled) {
        filter: brightness(0.9);
    }

    &:disabled {
        opacity: 0.7;
        cursor: not-allowed;
    }
}

.equipButtonSecondary {
    background: rgb(from var(--text-color-primary) r g b / 0.1);
    color: var(--text-color-primary);
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

.emptyState {
    margin-top: 12px;
    color: var(--text-color-secondary);
    font-size: 16px;
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.shopLink {
    width: fit-content;
    display: inline-block;
    color: var(--accent-color-primary-text);
    text-decoration: none;
    background: var(--accent-color-primary);
    border-radius: 12px;
    font-weight: 700;
    padding: 10px 18px;
}

.authModalHeader {
    margin-bottom: 24px;

    h3 {
        margin: 0;
        margin-bottom: 24px;
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
    transition: all 0.2s ease;
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

@media screen and (max-width: 500px) {
    .cardsGrid {
        grid-template-columns: 1fr;
    }
}
</style>
