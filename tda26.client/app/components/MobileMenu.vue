<script setup lang="ts">
import {computed} from "vue";
import {useRoute} from "#imports";
import {useState} from "#app";
import {NuxtLink, ClientOnly} from "#components";
import Menu from "~/components/Menu.vue";
import Avatar from "~/components/Avatar.vue";
import type {Account, Lecturer, WebTheme} from "#shared/types";

const mobileMenuOpened = useState<boolean>('mobileMenuOpened', () => false);
const loggedAccount = useState<Account | Lecturer | null>('loggedAccount', () => null);
const theme = useState<WebTheme>('theme', () => 'light');
const themeCookie = useCookie<WebTheme>('theme', {
    default: () => 'light',
    sameSite: 'lax',
    path: '/'
});

const currentPage = ref<string>("/");

watch(() => useRoute().path, (newPath) => {
    currentPage.value = newPath;
}, { immediate: true });

// pokud je mobileMenuOpened, tak se zablokuje scroll
watch(mobileMenuOpened, (newVal) => {
    if (newVal) {
        document.documentElement.style.overflow = 'hidden';
    } else {
        document.documentElement.style.overflow = '';
    }
});

function toggleTheme() {
    const newTheme: WebTheme = theme.value === 'light' ? 'dark' : 'light';
    theme.value = newTheme;
    themeCookie.value = newTheme;
}

async function logout() {
    mobileMenuOpened.value = false;
    navigateTo('/');
    
    try {
        await $fetch('/api/v2/auth/logout', {
            method: 'POST'
        });
    } catch (err) {
        console.error('Logout error:', err);
    } finally {
        navigateTo('/');
        loggedAccount.value = null;
    }
}
</script>

<template>
    <ClientOnly>
        <Transition name="mobile-menu-anim">
            <div :class="$style.mobileMenu" v-if="mobileMenuOpened">
                <div :class="$style.close" @click="mobileMenuOpened = false"></div>

                <div :class="$style.content">
                    <div :class="$style.logo"></div>

                    <!-- Profile section for logged-in users -->
                    <div v-if="loggedAccount" :class="$style.profileSection">
                        <Avatar :name="loggedAccount.fullNameWithoutTitles" :src="(loggedAccount as Lecturer).pictureUrl ?? null" :size="80" />
                        <div :class="$style.userInfo">
                            <p :class="$style.userName">{{ loggedAccount.fullNameWithoutTitles }}</p>
                            <p v-if='(loggedAccount as Lecturer)?.emails?.length ?? 0 > 0' :class="$style.userEmail">{{ (loggedAccount as Lecturer)?.emails?.[0] }}</p>
                        </div>
                    </div>

                    <!-- Menu -->
                    <nav>
                        <!-- Dashboard link for logged-in users -->
                        <NuxtLink v-if="loggedAccount" to="/dashboard" :class="$style.link" @click="mobileMenuOpened = false">Dashboard</NuxtLink>
                        
                        <Menu @itemClick="mobileMenuOpened = false" :link-class="$style.link" />
                    </nav>

                    <!-- Action buttons for logged-in users -->
                    <div v-if="loggedAccount" :class="$style.actionButtons">
                        <button :class="$style.actionButton" @click="toggleTheme">
                            <div :class="[$style.icon, $style.themeIcon]"></div>
                            <p>{{ theme === 'light' ? 'Tmavý režim' : 'Světlý režim' }}</p>
                        </button>
                        
                        <button :class="[$style.actionButton, $style.logoutButton]" @click="logout">
                            <div :class="[$style.icon, $style.logoutIcon]"></div>
                            <p>Odhlásit se</p>
                        </button>
                    </div>

<!--                    <LanguageSwitch />-->
                </div>
            </div>
        </Transition>
    </ClientOnly>
</template>

<style module lang="scss">
.mobileMenu {
    position: fixed;
    top: 0;
    width: 100vw;
    height: 100dvh;
    background-color: var(--background-color-primary);
    z-index: 20;
    transition-duration: 0.3s;
    overflow: hidden;

    >.close {
        position: absolute;
        top: 3vh;
        right: 7.5vw;
        width: 64px;
        height: 64px;
        cursor: pointer;
        mask: url('../../public/icons/x.svg');
        mask-size: 24px;
        mask-repeat: no-repeat;
        mask-position: center;
        background-color: var(--text-color-primary);
    }

    >.content {
        margin: 0 auto;
        margin-top: 14vh;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 64px;
        height: 100%;

        >.logo {
            width: 24vw;
            max-width: 196px;
            aspect-ratio: 1/1;
            mask-image: url('../../public/icons/Think-different-Academy_LOGO_erb.svg');
            mask-size: contain;
            mask-repeat: no-repeat;
            mask-position: center;
            background-color: var(--accent-color-primary);
        }

        nav {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 16px;
            width: 100%;

            .link {
                //text-transform: uppercase;
                font-weight: 600;
                transition-duration: 0.3s;
                cursor: pointer;
                text-decoration: none;
                font-size: 24px;
                color: var(--text-color);
                background-color: var(--background-color-2);
                padding: 16px;
                border-radius: 16px;
                width: 70%;
                max-width: 400px;
                text-align: center;

                &:hover {
                    color: var(--accent-color-primary-darker);
                    transition-duration: 0.3s;
                }

                &:is(:global(.router-link-active)) {
                    color: var(--accent-color-primary-text);
                    background-color: var(--accent-color-primary);
                }
            }
        }

        .profileSection {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 16px;
            padding: 24px;
            background-color: var(--background-color-2);
            border-radius: 16px;
            width: 70%;
            max-width: 400px;

            .userInfo {
                display: flex;
                flex-direction: column;
                align-items: center;
                gap: 4px;

                .userName {
                    margin: 0;
                    font-size: 20px;
                    font-weight: 700;
                    color: var(--text-color-primary);
                    text-align: center;
                }

                .userEmail {
                    margin: 0;
                    font-size: 14px;
                    color: var(--text-color-secondary);
                    text-align: center;
                }
            }
        }

        .actionButtons {
            display: flex;
            flex-direction: column;
            gap: 12px;
            width: 70%;
            max-width: 400px;

            .actionButton {
                display: flex;
                align-items: center;
                justify-content: center;
                gap: 12px;
                padding: 16px;
                background-color: var(--background-color-2);
                border: 1px solid transparent;
                border-radius: 16px;
                cursor: pointer;
                transition: all 0.3s ease;
                font-size: 18px;
                font-weight: 600;
                color: var(--text-color-primary);

                &:hover {
                    background-color: rgb(from var(--accent-color-primary) r g b / 0.1);
                    border-color: var(--accent-color-primary);
                    transform: translateY(-2px);
                }

                &.logoutButton {
                    &:hover {
                        background-color: rgba(220, 38, 38, 0.1);
                        border-color: rgb(220, 38, 38);
                    }
                }

                .icon {
                    width: 24px;
                    height: 24px;
                    mask-size: contain;
                    mask-position: center;
                    mask-repeat: no-repeat;
                    background-color: var(--text-color-primary);
                    flex-shrink: 0;

                    &.themeIcon {
                        mask-image: var(--theme-icon);
                    }

                    &.logoutIcon {
                        mask-image: url('/icons/logout.svg');
                    }
                }

                p {
                    margin: 0;
                }
            }
        }

        /*.switches {
            display: flex;
            align-items: center;
            gap: 24px;

            .langswitch {
                position: relative;
                cursor: pointer;
                background-color: var(--element-color-accent);
                transition-duration: 0.3s;
                padding: 8px;
                border-radius: 8px;

                &:hover {
                    .lang p {
                        color: var(--accent-color-darker);
                        transition-duration: 0.3s;
                    }

                    .lang >.icon {
                        filter: brightness(0.8);
                        transition-duration: 0.3s;
                    }
                }

                .lang {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    user-select: none;

                    >.icon {
                        width: 24px;
                        height: 16px;
                        background-size: cover;
                        background-position: center;
                        background-repeat: no-repeat;
                        border-radius: 4px;
                        transition-duration: 0.3s;
                    }

                    >p {
                        margin: 0;
                        font-weight: bold;
                        user-select: none;
                        color: var(--accent-color);
                        transition-duration: 0.3s;
                    }
                }
            }

            .themeswitch {
                width: 40px;
                position: relative;
                aspect-ratio: 1/1;
                cursor: pointer;
                background-color: var(--element-color-accent);
                border-radius: 100%;
                transition-duration: 0.3s;

                &:hover .themeswitch-icon {
                    background-color: var(--accent-color-darker);
                    transition-duration: 0.3s;
                }

                .themeswitch-icon {
                    width: 24px;
                    position: absolute;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    aspect-ratio: 1/1;
                    cursor: pointer;
                    background-color: var(--accent-color);
                    mask-size: contain;
                    mask-position: center;
                    mask-repeat: no-repeat;
                    transition-duration: 0.3s;
                }
            }
        }*/

        :deep(.lang-switch) .lang-menu {
            bottom: calc(100% + var(--ls-gap)) !important;
            top: unset;
        }

        .button {
            all: unset;
            cursor: pointer;
            user-select: none;
            font-family: "gabarito", sans-serif;
            font-size: 16px;
            padding: 12px 24px;
            background-color: var(--accent-color);
            color: var(--bg);
            border-radius: 8px;
            transition-duration: 0.3s;
            font-weight: bold;
            width: 110px;
            height: 20px;
            text-align: center;

            &:hover {
                background-color: var(--accent-color-darker);
                transition-duration: 0.3s;
            }
        }
    }
}
</style>

<style scoped lang="scss">
.mobile-menu-anim-enter-active, .mobile-menu-anim-leave-active {
    transition: 0.3s ease;
}

.mobile-menu-anim-enter-from, .mobile-menu-anim-leave-to {
    //opacity: 0;
    transform: translateX(100%);
}

.mobile-menu-anim-enter-to, .mobile-menu-anim-leave-from {
    transform: translateX(0%);
    //opacity: 1;
}
</style>