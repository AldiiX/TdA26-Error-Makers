<script setup lang="ts">
    const slot = useSlots();
</script>

<template>
    <button :class="[$style.button]">
        <slot />
    </button>
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
        transition: background-color 0.3s ease;

        &:hover {
            background-color: var(--button-bg-hover);
        }
    }
</style>