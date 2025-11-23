<script setup lang="ts">
    import { NuxtLink } from '#components';

    const props = withDefaults(defineProps<{
        linkClass?: string;
    }>(), {
        linkClass: ''
    });

    const emits = defineEmits<{
        (e: 'itemClick'): void
    }>();
</script>

<template>
    <NuxtLink to="/" :class="linkClass" @click="emits('itemClick')">Domů</NuxtLink>
    <NuxtLink to="/courses" :class="linkClass" @click="emits('itemClick')">Kurzy</NuxtLink>
    <NuxtLink to="/lecturers" :class="linkClass" @click="emits('itemClick')">Lektoři</NuxtLink>
    <NuxtLink to="/about" :class="linkClass" @click="emits('itemClick')">O nás</NuxtLink>
    <NuxtLink to="/faq" :class="linkClass" @click="emits('itemClick')">FAQ</NuxtLink>
</template>

<style module lang="scss">

</style>