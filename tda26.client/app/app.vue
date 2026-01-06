<script setup lang="ts">
import { NuxtPage, Head, Title, Meta, Html, Link } from '#components'
import { computed, onMounted, ref } from 'vue'
import { useNuxtApp, useRoute, useCookie, useHead } from '#imports'
import { NuxtLink, ClientOnly, NuxtLayout } from '#components';
import type {WebTheme} from "#shared/types";
import MobileMenu from "~/components/MobileMenu.vue";
import { Notivue, NotivueSwipe, Notification } from 'notivue';

// state
const route = useRoute();
const theme = useState<WebTheme>('theme', () => 'light');

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
    </NuxtLayout>
</template>

<style src="./app.scss" />