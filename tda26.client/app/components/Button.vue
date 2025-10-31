<!--suppress HtmlUnknownTarget -->
<script setup lang="ts">
    import { NuxtLink } from '#components';

    const slot = useSlots();

    type ButtonStyle = "primary" | "secondary" | "tertiary";

    const props = withDefaults(defineProps<{
        buttonStyle?: ButtonStyle,
        accentColor?: string,
        href?: string | null,
        target?: string | null,
        background?: string | null,
        style?: Record<string, string>,
        
    }>(), {
        buttonStyle: "primary",
        accentColor: 'var(--accent-color-primary)',
        href: null,
        target: null,
        background: null
    });
</script>

<template>
    <NuxtLink :href="href ?? undefined" :target="target ?? '_self'" :class="[$style.button, $style['style_' + buttonStyle] ]" :style="{ '--color': accentColor, '--bg': background ?? accentColor, ...style}">
        <slot />
    </NuxtLink>
</template>

<style module lang="scss">
    .button {
        background-color: var(--button-bg);
        color: var(--button-text-color);
        border: none;
        border-radius: 12px;
        padding: 12px 24px;
        font-size: 16px;
        font-family: Dosis, sans-serif;
        box-shadow: var(--button-shadow);
        cursor: pointer;
        transition-duration: 0.3s;
        user-select: none;
        text-decoration: none;

        &:focus, &:active {
            box-shadow: none;
            transition-duration: 0.3s;
        }


        &:hover {
            filter: brightness(0.75);
            transition-duration: 0.3s;
        }


        // jednotlive styly
        &:is(.style_primary) {
            background-color: var(--color);
            background: var(--bg);
            color: white;
        }

        &:is(.style_secondary) {
            background-color: transparent;
            color: var(--color);
            border: 2px solid var(--color);
        }
    }
</style>