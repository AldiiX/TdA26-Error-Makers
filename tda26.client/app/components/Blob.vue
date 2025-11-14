<script setup lang="ts">
    const props = withDefaults(defineProps<{
        size?: string,
        color?: string,
        top?: string,
        left?: string
        right?: string
        bottom?: string,
        blur?: string,
        background?: string,
        style?: Record<string, string>,
    }>(), {
        size: '20vw',
        color: 'var(--accent-color-primary)',
        top: 'unset',
        left: 'unset',
        right: 'unset',
        bottom: 'unset',
        blur: '0',
        background: 'linear-gradient(180deg, var(--accent-color-secondary-transparent-03), var(--accent-color))',
    });

    const randomPickedNumber = Math.floor(Math.random() * 4) + 1;
    const randomBlob = `url(/images/blobs/blob${randomPickedNumber}.svg)`;
</script>

<template>
    <div :class="$style.blobeffect" :style="style"></div>
</template>

<style module lang="scss">
    .blobeffect {
        position: absolute;
        top: v-bind(top);
        left: v-bind(left);
        right: v-bind(right);
        bottom: v-bind(bottom);
        width: v-bind(size);
        aspect-ratio: 1/1;
        background: v-bind(background);
        mask-image: v-bind(randomBlob);
        mask-size: contain;
        mask-position: center;
        mask-repeat: no-repeat;
        filter: blur(v-bind(blur));
        z-index: -1;
    }
</style>