<script setup lang="ts">
    import { NuxtLink } from '#components';

    const props = withDefaults(defineProps<{
        linkClass?: string;
    }>(), {
        linkClass: ''
    });
</script>

<template>
    <NuxtLink to="/" :class="linkClass">Domů</NuxtLink>
    <NuxtLink to="/courses" :class="linkClass">Kurzy</NuxtLink>
    <NuxtLink to="/lecturers" :class="linkClass">Lektoři</NuxtLink>
    <NuxtLink to="/about" :class="linkClass">O nás</NuxtLink>
    <NuxtLink to="/faq" :class="linkClass">FAQ</NuxtLink>
</template>

<style scoped lang="scss">

</style>