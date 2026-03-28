<script setup lang="ts">
import { ClientOnly } from '#components';
import LecturerCard from "~/components/pagespecific/LecturerCard.vue";
import type {Lecturer} from "#shared/types";
import TypeWriter from "~/components/TypeWriter.vue";
import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
import Blob from "~/components/Blob.vue";
import getBaseUrl from "#shared/utils/getBaseUrl";
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";

definePageMeta({
    layout: "normal-page-layout"
});

// SEO
useSeo({
    title: "Obchod",
});

const { data: items } = useAsyncData("shop-items", async () => {
    const res = await fetch(getBaseUrl() + "/api/v1/shop/");
    return res.json() as Promise<any[]>;
}, { server: false });
</script>

<template>
    <Teleport to="#teleports">
        <CircleBlurBlob top="12.5vh" right="-12.5vw" color="var(--accent-color-primary)" size="20vw" blur="14vw" />
        <CircleBlurBlob top="100vh" left="-12.5vw" color="var(--accent-color-secondary-theme)" size="20vw" blur="14vw" />
    </Teleport>



    <section>
        <h1 :class="[$style.nadpis/*, 'text-gradient'*/]">Obchod</h1>
        <ClientOnly>
            <SmoothSizeWrapper :change-width="false">
                <TypeWriter
                    text="Zakupte si zajímavé položky pro váš účet! Avatary, efekty, bannery... vše na jednom místě!"
                    :class="$style.podnapis"
                    :start-delay-ms="300"
                />
            </SmoothSizeWrapper>
        </ClientOnly>

        <div :class="$style.parent">
            <div :class="$style.child" v-for="item in items">
                <p>{{ item }}</p>
            </div>
        </div>
    </section>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

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
    //width: fit-content;
    margin:0;
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
    height: 46px;
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

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
    .nadpis, .podnapis {
        text-align: center;
        margin: 0 auto;
    }
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
    section {
        margin-top: -50px;
    }

    .list {
        display: flex;
        flex-direction: column;
    }
}

@media screen and (max-width: 750px) {
    .podnapis {
        height: 60px;
    }
}

@media screen and (max-width: 500px) {
    .podnapis {
        height: 110px;
    }
}

@media screen and (max-width: 350px) {
    .podnapis {
        height: 150px;
    }
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
}
</style>