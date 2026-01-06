<script setup lang="ts">
    import { navigateTo } from "#app";
    import { definePageMeta } from "#imports";

    definePageMeta({
        layout: "normal-page-layout",
        middleware: () => {
            return navigateTo('/courses');
        }
    });
</script>

<template>

</template>

<style scoped lang="scss">

</style>