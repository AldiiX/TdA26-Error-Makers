<script setup lang="ts">
    import { Head, Title } from '#components';

    definePageMeta({
        layout: "normal-page-layout"
    });
</script>

<template>
    <Head>
        <Title>Lektoři • Think different Academy</Title>
    </Head>

    <h1 :class="$style.nadpis">Lektoři</h1>
    <p :class="$style.podnapis">Podívej se na naše úžasné lektory, kteří tě provedou světem technologií a programování. Každý z nich přináší jedinečný přístup k výuce, který ti pomůže dosáhnout tvých cílů.</p>
</template>

<style module lang="scss">
.nadpis {
    font-size: 64px;
    margin:0;
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
}
</style>