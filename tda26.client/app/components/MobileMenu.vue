<script setup lang="ts">
import {computed} from "vue";
import {useRoute} from "#imports";
import {useState} from "#app";
import {NuxtLink, ClientOnly} from "#components";
import Menu from "~/components/Menu.vue";
import type {Account, Lecturer, WebTheme} from "#shared/types";
import Avatar from "~/components/Avatar.vue";


const loggedAccount = useState<Account | Lecturer | null>('loggedAccount', () => null);
const mobileMenuOpened = useState<boolean>('mobileMenuOpened', () => false);
const currentPage = ref<string>("/");
const theme = useState<WebTheme>('theme', () => 'light');

const themeCookie = useCookie<WebTheme>('theme', {
    default: () => 'light',
    sameSite: 'lax',
    path: '/'
});

function toggleTheme() {
    const newTheme: WebTheme = theme.value === 'light' ? 'dark' : 'light';
    theme.value = newTheme;
    themeCookie.value = newTheme;
}

async function logout() {
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
</script>

<template>
    <ClientOnly>
        <Transition name="mobile-menu-anim">
            <div :class="$style.mobileMenu" v-if="mobileMenuOpened">
                <div :class="$style.close" @click="mobileMenuOpened = false"></div>

                <div :class="$style.content">
                    <div :class="$style.logo"></div>

                    <div :class="$style.loggedAs">
                        
                        <template v-if="loggedAccount">
                            <div :class="$style.accountHeader">
                                <Avatar
                                    :name="loggedAccount.fullNameWithoutTitles"
                                    :src="(loggedAccount as Lecturer).pictureUrl ?? null"
                                    :size="72"
                                />

                                <div :class="$style.accountText">
                                    <p :class="$style.label">Přihlášen jako</p>
                                    <p :class="[$style.name, 'text-gradient']">
                                        {{ loggedAccount.fullNameWithoutTitles }}
                                    </p>
                                </div>
                            </div>

                            <div :class="$style.accountActions">
                                <button :class="$style.actionButton" @click="toggleTheme">
                                    <div :class="[$style.icon, $style.themeIcon]"></div>
                                    <span>{{ theme === 'light' ? 'Tmavý režim' : 'Světlý režim' }}</span>
                                </button>

                                <button :class="[$style.actionButton, $style.logout]" @click="logout">
                                    <div :class="[$style.icon, $style.logoutIcon]"></div>
                                    <span>Odhlásit se</span>
                                </button>
                            </div>
                            
                        </template>

                        <template v-else>
                            <div :class="$style.notLogged">
                                <p class="title">Přihlášení</p>
                                <p class="hint">Nemáš účet? Vytvoř si ho</p>

                                <div :class="$style.authButtons">
                                    <NuxtLink to="/login" @click="mobileMenuOpened = false">
                                        <Button button-style="primary">Přihlásit se</Button>
                                    </NuxtLink>

                                    <NuxtLink to="/register" @click="mobileMenuOpened = false">
                                        <Button button-style="secondary">Registrovat se</Button>
                                    </NuxtLink>
                                </div>
                            </div>
                        </template>
                    </div>
                    
                    <!-- Menu -->
                    <nav>
                        <NuxtLink
                            v-if="loggedAccount"
                            to="/dashboard"
                            :class="$style.link"
                            @click="mobileMenuOpened = false"
                        >
                            Dashboard
                        </NuxtLink>
                        <Menu @itemClick="mobileMenuOpened = false" :link-class="$style.link" />
                    </nav>

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
        padding-bottom: 64px;
        overflow-y: auto;

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
        
        .loggedAs {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 32px;
            width: 100%;

            .accountHeader {
                display: flex;
                align-items: center;
                gap: 16px;
            }

            .accountText {
                text-align: left;
                
                .label {
                    margin: 0;
                    font-size: 16px;
                    opacity: 0.75;
                }

                .name {
                    margin: 0;
                    font-size: 24px;
                    font-weight: 600;
                }
            }

            .accountActions {
                display: flex;
                gap: 32px;

                .actionButton {
                    all: unset;
                    cursor: pointer;
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    font-size: 18px;
                    color: var(--text-color);
                    transition-duration: 0.3s;
                    

                    .icon {
                        width: 20px;
                        height: 20px;
                        background-color: var(--text-color);
                    }

                    .themeIcon {
                        mask-image: var(--theme-icon);
                        mask-size: contain;
                        mask-repeat: no-repeat;
                        mask-position: center;
                    }

                    .logoutIcon {
                        mask: url('../../public/icons/logout.svg');
                        mask-size: contain;
                        mask-repeat: no-repeat;
                        mask-position: center;
                    }
                    
                }

                .logout {
                    margin-left: auto;
                }
            }

            
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