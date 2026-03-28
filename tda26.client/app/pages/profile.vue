<script setup lang="ts">
import { NuxtLink } from "#components";
import type { Account, ProfilePayload, ShopItem } from "#shared/types";
import formatCzechCount from "#shared/utils/formatCzechCount";
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
import LoginForm from "~/components/LoginForm.vue";
import Modal from "~/components/Modal.vue";
import RegisterForm from "~/components/RegisterForm.vue";
import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";

definePageMeta({
    layout: "normal-page-layout"
});

useSeo({
    title: "Profil"
});

const loggedAccount = useState<Account | null>("loggedAccount", () => null);
const authModalOpen = ref(false);
const authTab = ref<"login" | "register">("login");

const { data: profileData, pending, refresh } = useAsyncData("me-profile-page", async () => {
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

onMounted(() => {
    if (!loggedAccount.value) {
        authModalOpen.value = true;
        authTab.value = "login";
    }
});

const equippedRows = computed(() => {
    const equipped = profileData.value?.equipped;
    return [
        { slot: "Avatar", item: equipped?.avatar ?? null },
        { slot: "Banner", item: equipped?.banner ?? null },
        { slot: "Efekt", item: equipped?.effect ?? null },
        { slot: "Odznak", item: equipped?.badge ?? null },
        { slot: "Titul", item: equipped?.title ?? null }
    ];
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

function itemImage(item: ShopItem | null): string | null {
    return item?.imageUrl ?? null;
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
        <h1 :class="$style.nadpis">Profil</h1>

        <div v-if="!loggedAccount" :class="$style.guestNotice">
            Pro zobrazení profilu se přihlas.
            <button type="button" :class="$style.inlineAction" @click="authModalOpen = true">Přihlásit se</button>
        </div>

        <template v-else>
            <div v-if="pending" :class="$style.loading">Načítám profil...</div>

            <div v-else-if="profileData" :class="$style.grid">
                <article :class="$style.card">
                    <h3>Základní informace</h3>
                    <p><strong>Uživatel:</strong> {{ profileData.account.fullNameWithoutTitles }}</p>
                    <p><strong>UUID:</strong> {{ profileData.account.uuid }}</p>
                    <p><strong>Kačenky:</strong> {{ ducksLabel(profileData.account.ducks) }}</p>
                    <p><strong>XP:</strong> {{ profileData.account.xp }}</p>
                    <p><strong>Level:</strong> {{ profileData.account.level }}</p>
                    <NuxtLink to="/inventory" :class="$style.linkButton">Otevřít inventář</NuxtLink>
                </article>

                <article :class="$style.card">
                    <h3>Vybavení</h3>
                    <div :class="$style.equipmentList">
                        <div v-for="row in equippedRows" :key="row.slot" :class="$style.equipmentRow">
                            <div :class="$style.slotName">{{ row.slot }}</div>
                            <div :class="$style.slotItem">
                                <template v-if="row.item">
                                    <img v-if="itemImage(row.item)" :src="itemImage(row.item) ?? undefined" :alt="row.item.name" :class="$style.itemPreview">
                                    <div>
                                        <p :class="$style.itemName">{{ row.item.name }}</p>
                                        <p :class="$style.itemMeta">{{ row.item.type }} · {{ row.item.priceInDucks }}</p>
                                    </div>
                                </template>
                                <p v-else :class="$style.notEquipped">Nevybaveno</p>
                            </div>
                        </div>
                    </div>
                </article>

                <article :class="$style.card">
                    <h3>Inventář</h3>
                    <p>Počet položek: {{ profileData.inventory.length }}</p>
                    <div :class="$style.inventoryPreview">
                        <div v-for="item in profileData.inventory.slice(0, 6)" :key="item.uuid" :class="$style.previewItem">
                            <img v-if="item.imageUrl" :src="item.imageUrl" :alt="item.name">
                            <span v-else>{{ item.name.charAt(0) }}</span>
                        </div>
                    </div>
                    <NuxtLink to="/shop" :class="$style.linkButtonSecondary">Přejít do obchodu</NuxtLink>
                </article>
            </div>

            <div v-else :class="$style.loading">
                Profil se nepodařilo načíst.
                <button type="button" :class="$style.inlineAction" @click="() => refresh()">Zkusit znovu</button>
            </div>
        </template>

        <Modal
            :enabled="authModalOpen"
            can-be-closed-by-clicking-outside
            :modal-style="{ maxWidth: '500px' }"
            @close="authModalOpen = false"
        >
            <SmoothSizeWrapper>
                <div :class="$style.authModalHeader">
                    <h3>Pro zobrazení profilu se musíš přihlásit.</h3>

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
                            <LoginForm v-if="authTab === 'login'" @login-success="handleAuthSuccess" />
                            <RegisterForm v-else @register-success="handleAuthSuccess" />
                        </div>
                    </Transition>
                </div>
            </SmoothSizeWrapper>
        </Modal>
    </section>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.section {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.nadpis {
    margin: 0;
    font-size: 64px;
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
    margin-left: 8px;
    border: none;
    background: transparent;
    color: inherit;
    font-size: 14px;
    font-weight: 800;
    cursor: pointer;
    border-bottom: 1px solid currentColor;
    font-family: Dosis, sans-serif;
}

.grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
    gap: 24px;
}

.card {
    border-radius: 20px;
    padding: 18px;
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 4px 30px rgba(0, 0, 0, 0.15);
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 0.6);
    backdrop-filter: blur(8px) saturate(1.6);

    h3 {
        margin: 0 0 12px;
    }

    p {
        margin: 6px 0;
    }
}

.loading {
    color: var(--text-color-secondary);
}

.linkButton,
.linkButtonSecondary {
    margin-top: 12px;
    display: inline-block;
    width: fit-content;
    text-decoration: none;
    border-radius: 10px;
    padding: 10px 14px;
    font-weight: 700;
}

.linkButton {
    background: var(--accent-color-primary);
    color: var(--accent-color-primary-text);
}

.linkButtonSecondary {
    background: var(--accent-color-secondary-theme);
    color: var(--accent-color-secondary-theme-text);
}

.equipmentList {
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.equipmentRow {
    display: grid;
    grid-template-columns: 72px 1fr;
    align-items: center;
    gap: 10px;
}

.slotName {
    font-weight: 700;
    color: var(--text-color-secondary);
}

.slotItem {
    display: flex;
    align-items: center;
    gap: 10px;
}

.itemPreview {
    width: 52px;
    height: 52px;
    border-radius: 8px;
    object-fit: cover;
}

.itemName {
    margin: 0;
    font-weight: 700;
}

.itemMeta {
    margin: 0;
    font-size: 13px;
    color: var(--text-color-secondary);
}

.notEquipped {
    color: var(--text-color-secondary);
    margin: 0;
}

.inventoryPreview {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 8px;
    margin-top: 10px;
}

.previewItem {
    border-radius: 10px;
    min-height: 68px;
    background: rgb(from var(--background-color-3) r g b / 0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    overflow: hidden;

    img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    span {
        font-weight: 800;
        font-size: 22px;
        color: var(--accent-color-primary);
    }
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

@media screen and (max-width: app.$tabletBreakpoint) {
    .section {
        margin-top: -50px;
    }
}
</style>


