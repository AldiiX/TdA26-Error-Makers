<script setup lang="ts">
    import { ClientOnly } from '#components';
    import LecturerCard from "~/components/pagespecific/LecturerCard.vue";
    import type {Lecturer} from "#shared/types";
    import TypeWriter from "~/components/TypeWriter.vue";
    import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
    import Blob from "~/components/Blob.vue";
    import getBaseUrl from "#shared/utils/getBaseUrl";

    definePageMeta({
        layout: "normal-page-layout"
    });

    // SEO
    useSeo({
        title: "Lektoři",
        description: "Prohlédněte si profily našich lektorů a expertů. Každý lektor přináší své znalosti a zkušenosti do kurzů na Think Different Academy.",
        keywords: "lektoři, učitelé, instruktoři, vyučující, experti"
    });

    const { data: lecturers, pending: lecturersFetchPending, error: lecturersFetchError } = useFetch<Lecturer[]>(getBaseUrl() + '/api/v2/lecturers', {
        server: false,
        key: 'lecturers-list',
    });
</script>

<template>
    <!-- blobíci -->
    <Teleport to="#teleports">
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
    <ClientOnly>
        <SmoothSizeWrapper>
            <TypeWriter
                    style="height: 46px"
                    text="Podívej se na naše úžasné lektory, kteří tě provedou světem kurzů. Každý z nich přináší jedinečný přístup k výuce, který ti pomůže dosáhnout tvých cílů."
                    :class="$style.podnapis"
                    :startDelayMs="300"
            />
        </SmoothSizeWrapper>
    </ClientOnly>

<!--    <div :class="$style.list">-->
<!--        <LecturerCard v-for="_ in 6" :lecturer="null" :class="[$style.card, $style.cardloading]" />-->
<!--    </div>-->

    <div :class="$style.list">
        <!-- lectureri plně načteni -->
        <LecturerCard v-if="lecturers && !lecturersFetchPending && !lecturersFetchError" v-for="l in lecturers" :lecturer="l" :class="$style.card" />

        <!-- načítání lektorů -->
        <LecturerCard v-else-if="lecturersFetchPending" v-for="_ in 6" :lecturer="null" :class="$style.card" />

        <!-- chyba pri nacitani lektoru -->
        <div v-if="lecturersFetchError">
            <p>Něco se pokazilo při načítání lektorů. Zkus to prosím znovu později.</p>
        </div>
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
    margin-bottom: 32px;
    animation: sddjfiosdjfujdfibnmijfgbno 1s forwards ease;

    @keyframes sddjfiosdjfujdfibnmijfgbno {
        from {
            opacity: 0;
            transform: translateY(24px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    .card {
        /*&:nth-child(odd) {
            .name {
                color: var(--accent-color-secondary-theme);
            }
        }*/

        &:is(.cardloading) {
            //height: 164px;
        }

        .name {
            color: var(--accent-color-primary);
        }
    }
}
</style>