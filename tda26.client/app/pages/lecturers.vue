<script setup lang="ts">
    import { Head, Title } from '#components';
    import LecturerCard from "~/components/pagespecific/LecturerCard.vue";
    import type {Lecturer} from "#shared/types";
    import TypeWriter from "~/components/TypeWriter.vue";
    import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
    import Blob from "~/components/Blob.vue";

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


    <!-- blobíci -->
    <Teleport to="#__nuxt">
        <div :class="$style.blobeffect"></div>
        <Blob
            top="60vh"
            right="2vw"
            left="unset"
            background="linear-gradient(0deg, var(--accent-color-primary) 0%, transparent 80%)"
            style="opacity: 0.5"
            :class="[$style.blob, $style.blob1]"
        />

        <Blob
            top="100vh"
            left="2vw"
            right="unset"
            background="linear-gradient(0deg, transparent 0%, var(--accent-color-secondary-theme)  80%)"
            style="opacity: 0.5"
            :class="[$style.blob, $style.blob2]"
        />
    </Teleport>



    <h1 :class="[$style.nadpis/*, 'text-gradient'*/]">Lektoři</h1>
    <SmoothSizeWrapper>
        <TypeWriter
                style="height: 46px"
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
.blobeffect {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 80%;
    background: linear-gradient(180deg, var(--accent-color-secondary-transparent-03), var(--accent-color));
    mask-image: url('../../public/images/eff1.svg');
    mask-size: cover;
    mask-position: center;
    mask-repeat: no-repeat;
    z-index: -1;
    animation: sdoksapkdf 1.5s forwards ease;

    @keyframes sdoksapkdf {
        from {
            transform: translateY(-80px);
            opacity: 0;
        }
        to {
            transform: translateY(0);
            opacity: 1;
        }
    }
}

.blob1 {
    animation: asdsasafasfasfhhdmg 3s forwards ease;

    @keyframes asdsasafasfasfhhdmg {
        0% {
            opacity: 0;
            transform: translateX(80px);
        }

        20% {
            opacity: 0;
            transform: translateX(80px);
        }

        to {
            opacity: 0.5;
            transform: translateX(0);
        }
    }
}

.blob2 {
    animation: sdpofksdpofk 3s forwards ease;

    @keyframes sdpofksdpofk {
        0% {
            opacity: 0;
            transform: translateX(-80px);
        }

        20% {
            opacity: 0;
            transform: translateX(-80px);
        }

        to {
            opacity: 0.5;
            transform: translateX(0);
        }
    }
}

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