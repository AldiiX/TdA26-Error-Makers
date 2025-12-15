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

// seo base
const description = "ASP.NET Core + Nuxt template";

// robots podle prostredi, aby se neindexoval dev/staging
const isProd = import.meta.env.PROD;
const robotsValue = computed(() => isProd ? "index, follow" : "noindex, nofollow");

// kanonicka url bez query, aby se nekanonizovaly utm apod.
// const canonicalUrl = computed(() => new URL(route.path || "/", siteUrl).toString());


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
</script>

<template>
    <!-- spravne jazykove nastaveni na html -->
<!--    <Html :lang="mappedLang" />-->

    <Head>
        <!-- zaklad -->
        <Meta charset="utf-8" />
        <Meta name="viewport" content="width=device-width, initial-scale=1" />
        <Title>Think different Academy</Title>
        <Meta name="description" :content="description" />
        <Meta name="robots" :content="robotsValue" />
        <Link rel="stylesheet" href="/lib/pure.css" type="text/css" />
<!--        <Link rel="canonical" :href="canonicalUrl" />-->

        <!-- hreflang alternace -->
<!--        <Link rel="alternate" hreflang="x-default" :href="siteUrl" />-->
<!--        <Link rel="alternate" hreflang="cs" :href="siteUrl" />-->

        <!-- open graph -->
        <Meta property="og:type" content="website" />
<!--        <Meta property="og:title" :content="pageTitle" />-->
        <Meta property="og:description" :content="description" />
<!--        <Meta property="og:url" :content="canonicalUrl" />-->
<!--        <Meta property="og:image" :content="ogImage" />-->
<!--        <Meta property="og:locale" :content="ogLocale" />-->

        <!-- twitter / x cards -->
        <Meta name="twitter:card" content="summary_large_image" />
<!--        <Meta name="twitter:title" :content="pageTitle" />-->
        <Meta name="twitter:description" :content="description" />
<!--        <Meta name="twitter:image" :content="ogImage" />-->

        <!-- pwa/chrome ui barva -->
        <Meta name="theme-color" content="#0095ff" media="(prefers-color-scheme: dark)" />
        <Meta name="theme-color" content="#0095ff" media="(prefers-color-scheme: light)" />
<!--        <Link rel="manifest" href="/site.webmanifest" />-->
    </Head>

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