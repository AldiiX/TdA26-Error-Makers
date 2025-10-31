<script setup lang="ts">
    import { Head, Title } from '#components';

    definePageMeta({
        layout: "normal-page-layout"
    });
</script>

<template>
    <Head>
        <Title>Kurzy • Think different Academy</Title>
    </Head>

    <h1 :class="$style.nadpis">Kurzy</h1>
    <p :class="$style.podnapis">Objev naši širokou nabídku kurzů, které pokrývají různé oblasti technologií a programování. Ať už jsi začátečník nebo pokročilý, máme pro tebe kurz, který ti pomůže rozvíjet tvé dovednosti a znalosti.</p>
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