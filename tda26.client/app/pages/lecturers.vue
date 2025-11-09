<script setup lang="ts">
    import { Head, Title } from '#components';
    import LecturerCard from "~/components/pagespecific/LecturerCard.vue";
    import type {Lecturer} from "~/lib/types";
    import TypeWriter from "~/components/TypeWriter.vue";
    import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";

    definePageMeta({
        layout: "normal-page-layout"
    });

    const { data: lecturers } = await useAsyncData(
        'lecturers',
        () => $fetch<Lecturer[]>('/api/v2/lecturers')
    )
</script>

<template>
    <Head>
        <Title>Lektoři • Think different Academy</Title>
    </Head>

    <h1 :class="[$style.nadpis, 'text-gradient']">Lektoři</h1>
    <SmoothSizeWrapper>
        <TypeWriter
                text="Podívej se na naše úžasné lektory, kteří tě provedou světem kurzů. Každý z nich přináší jedinečný přístup k výuce, který ti pomůže dosáhnout tvých cílů."
                :class="$style.podnapis"
                :startDelayMs="300"
        />
    </SmoothSizeWrapper>

    <div :class="$style.list">
        <LecturerCard v-for="l in lecturers" :lecturer="l" :class="$style.card" />
    </div>
</template>

<style module lang="scss">
.nadpis {
    font-size: 64px;
    width: fit-content;
    margin:0;
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
}

.list {
    // max dva sloupce, celkem 100%
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(600px, 1fr));
    gap: 24px;
    margin-top: 32px;

    .card {
        /*&:nth-child(odd) {
            .name {
                color: var(--accent-color-secondary-theme);
            }
        }*/

        .name {
            color: var(--accent-color-primary);
        }
    }
}
</style>