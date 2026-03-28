<script setup lang="ts">
import { NuxtPage, Head, Title, Meta, Html, Link, NuxtLink, ClientOnly, NuxtLayout  } from '#components'
import { computed, onMounted, ref } from 'vue'
import { useNuxtApp, useRoute, useCookie, useHead } from '#imports'

import type { Account, WebTheme } from "#shared/types";
import MobileMenu from "~/components/MobileMenu.vue";
import LoadingScreen from "~/components/LoadingScreen.vue";
import { Notivue, NotivueSwipe, Notification } from 'notivue';

// state
const route = useRoute();
const theme = useState<WebTheme>('theme', () => 'light');
const loggedAccount = useState<Account | null>("loggedAccount", () => null);
const debugAccountBadgeLabel = computed<string | null>(() => {
    if (!loggedAccount.value?.type) return null;

    const type = loggedAccount.value.type;
    // Admin must never be treated/displayed as premium.
    const isPremium = type !== "admin" && loggedAccount.value.isPremium === true;

    return isPremium ? `${type} (premium)` : type;
});
const isDev = import.meta.dev;

// Default SEO fallback - pages should use useSeo() composable for specific SEO
const siteTitle = "Think Different Academy";
const description = "Interaktivní vzdělávací platforma pro studenty a lektory. Objevte kurzy, kvízy a mnoho dalšího na Think Different Academy.";

// head: data-theme na <html> a zakladni link/script
useHead({
    htmlAttrs: { 'data-theme': computed(() => theme.value ?? undefined) },

    link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'preload', as: 'font', type: 'font/ttf', href: '/fonts/dosis.ttf', crossorigin: 'anonymous' },
    ],

    script: [
        { // recaptcha
            src: 'https://www.google.com/recaptcha/api.js?render=6LfDQhksAAAAAEz_ujbJNian3-e-TfyKx8gzRaCL',
            async: true,
            defer: true,
        },
    ],

    noscript: [

    ]
});

// Default SEO meta tags (will be overridden by page-specific useSeo())
useSeo({
    title: undefined, // Will use site name only
    description: description
});
</script>

<template>
    <div v-if="isDev && debugAccountBadgeLabel" :class="$style.accountTypeDebug" role="status" aria-live="polite">
        Debug: logged in as <strong>{{ debugAccountBadgeLabel }}</strong>
    </div>

    <!-- Mobile menu -->
    <MobileMenu />

    <!-- toasty přes notivue -->
    <Notivue v-slot="item">
        <NotivueSwipe :item="item">
            <Notification :item="item" />
        </NotivueSwipe>
    </Notivue>

    <NuxtLayout>
        <NuxtPage />
        
        <div :class="$style.smallDevice">
            <p>Šířka zařízení je příliš malá. Použijte prosím zařízení s větším rozlišením nebo šířkou obrazovky.</p>
        </div>
    </NuxtLayout>
</template>

<style module lang="scss">
.smallDevice {
    display: none;
}

@media screen and (max-width: 299px) {
    .main {
        display: none;
    }

    .smallDevice {
        position: fixed;
        top: 0;
        left: 0;
        height: 100vh;
        width: 100vw;
        background-color: white;
        z-index: 1000000;
        display: flex;
        justify-content: center;
        align-items: center;

        p {
            font-size: 18px;
            text-align: center;
            padding: 20px;
        }
    }
}

.accountTypeDebug {
    position: fixed;
    top: 8px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 2000;
    font-size: 12px;
    background: rgb(from var(--background-color-secondary) r g b / 0.9);
    border: 1px solid rgb(from var(--text-color-primary) r g b / 0.2);
    color: var(--text-color-primary);
    border-radius: 999px;
    padding: 6px 10px;
    pointer-events: none;
}
</style>

<style lang="scss">
html,
body,
#__nuxt {
    font-family: "Dosis", sans-serif;
}
</style>
