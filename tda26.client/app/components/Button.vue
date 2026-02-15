<!--suppress HtmlUnknownTarget -->
<script setup lang="ts">
    import { NuxtLink } from '#components';

    const slot = useSlots();

    type ButtonStyle = "primary" | "secondary" | "tertiary";

    const props = withDefaults(defineProps<{
        buttonStyle?: ButtonStyle,
        accentColor?: string | "primary" | "secondary" | "gradient",
        textColor?: string | null,
        background?: string | null,
        style?: Record<string, string>,
        loading?: boolean,
        disabled?: boolean,
    }>(), {
        buttonStyle: "primary",
        accentColor: 'primary',
        background: null,
        textColor: null,
        loading: false,
        disabled: false,
    });

    const emit = defineEmits<{
        (e: 'click', event: MouseEvent): void;
    }>();
</script>

<template>
    <button
            :disabled="disabled"
            :class="[$style.button, $style['style_' + buttonStyle], { [$style.loading]: loading }]"
            :style="{
                '--color': accentColor === 'primary' ? 'var(--accent-color-primary)' : accentColor === 'secondary' ? 'var(--accent-color-secondary-theme)' : accentColor,
                '--bg': accentColor === 'gradient' ? 'linear-gradient(60deg, var(--accent-color-primary), var(--accent-color-secondary-theme))' : background ?? accentColor,
                '--txc': (textColor) ?? (accentColor === 'primary' ? 'var(--accent-color-primary-text)' : accentColor === 'secondary' || accentColor === 'gradient' ? 'var(--accent-color-secondary-theme-text)' : 'inherit'),
                ...style
            }"
            @click="disabled ? null : $emit('click', $event)">

        <template v-if="!loading">
            <slot />
        </template>

        <template v-else>
            Načítání...
        </template>
    </button>
</template>

<style module lang="scss">
    .button {
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
        text-align: center;

        &:is(.loading) {
            pointer-events: none;
            opacity: 0.7;
        }

        &:disabled {
            cursor: not-allowed;
            opacity: 0.5;
        }

        &:focus, &:active {
            box-shadow: none;
            transition-duration: 0.3s;
        }


        &:not(:disabled):hover {
            filter: brightness(0.75);
            transition-duration: 0.3s;
        }


        // jednotlive styly
        &:is(.style_primary) {
            background-color: var(--color) !important;
            background: var(--bg);
            color: var(--txc);
            border: 2px solid var(--color);
        }

        &:is(.style_secondary) {
            background-color: transparent;
            color: var(--color);
            border: 2px solid var(--color);
        }
    }
</style>