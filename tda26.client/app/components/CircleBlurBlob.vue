<script setup lang="ts">
    const props = withDefaults(defineProps<{
        size?: string,
        color?: string,
        top?: string,
        left?: string
        right?: string
        bottom?: string,
        blur?: string,
    }>(), {
        size: '20vw',
        color: 'var(--accent-color-primary)',
        top: 'unset',
        left: 'unset',
        right: 'unset',
        bottom: 'unset',
        blur: '15vw',
    });
</script>

<template>
    <div :class="$style.one"></div>
</template>

<style module lang="scss">
.one {
    overflow: hidden;
    position: absolute;
    aspect-ratio: 1/1;
    background-color: v-bind(color);
    top: v-bind(top);
    right: v-bind(right);
    left: v-bind(left);
    bottom: v-bind(bottom);
    width: v-bind(size);
    border-radius: 100%;
    animation: blobanim 10s infinite ease;
    filter: blur(v-bind(blur));
}

@keyframes blobanim {
    0%,100% {
        transform: scale(1);
    }
    50% {
        transform: scale(1.18);
    }
}
</style>