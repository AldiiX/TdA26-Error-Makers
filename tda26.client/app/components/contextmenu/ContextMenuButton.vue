<script setup lang="ts">
const emit = defineEmits<{
    (e: "open", event: MouseEvent): void
}>()
</script>

<template>
    <span
        :class="[$style.contextMenuButton]"
        @click="$emit('open', $event)"
    />
</template>

<style module lang="scss">
.liquid-glass {
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 4px 30px rgba(0, 0, 0, 0.15);
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    backdrop-filter: blur(8px) saturate(1.6);
}

.contextMenuButton {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background-color: rgba(255, 255, 255, 0.3);
    transition-duration: 0.3s;
    cursor: pointer;
    user-select: none;
    transition: all 0.3s;
    @extend .liquid-glass;

    &::before {
        content: '';
        display: block;
        width: 20px;
        height: 20px;
        background-color: black;
        mask-size: contain;
        mask-position: center;
        mask-repeat: no-repeat;
        mask-image: url("/icons/ellipsis.svg");
    }

    &:hover {
        background-color: rgba(255, 255, 255, 0.7);
        transition-duration: 0.3s;
    }
}
</style>