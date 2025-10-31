<script setup lang="ts">
    import Header from "~/components/Header.vue";
    import Footer from "~/components/Footer.vue";

    const slot = useSlots();
</script>

<template>
    <Header />

    <main :class="$style.main">
        <slot />
    </main>

    <Footer />
</template>

<style module lang="scss">
.main {
    min-height: calc(100vh - 400px);
    display: flex;
    flex-direction: column;
    width: 80%;
    box-sizing: border-box;
    margin: 0 auto;
    margin-top: 200px;
}
</style>