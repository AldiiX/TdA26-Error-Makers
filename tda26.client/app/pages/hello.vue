<script setup lang="ts">
    import Header from "~/components/Header.vue";
    import Footer from "~/components/Footer.vue";
    import { Head, Title } from '#components';

    const theme = useState("theme");

    definePageMeta({
        alias: "/"
    })
</script>




<template>
    <Head>
        <Title>Hello TdA • Think different Academy</Title>
    </Head>

<!--    <Header/>-->

    <section :class="$style.top">
        <div :class="$style.blob" :style="{ opacity: (theme == 'dark' ? '0.5' : '') }"></div>

        <div :class="$style.center">
            <h1><span>&lt</span>Hello TdA<span>/&gt</span></h1>
            <p>We are Error Makers!</p>
        </div>
    </section>

<!--    <Footer/>-->
</template>


<style module lang="scss">
@use "../app" as app;

@keyframes ospfpodskfposdkfpsok {
    from { opacity: 0; margin-top: 100px; }
    to { opacity: 1; margin-top: 0;}
}

section:is(.top) {
    position: relative;
    height: calc(100vh);
    width: 100%;
    overflow: hidden;

    .blob {
        position: absolute;
        margin-top: 15vh;
        width: 100%;
        aspect-ratio: 16/9;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        mask: url('../../public/images/blob1.svg') center/cover no-repeat;
        background: linear-gradient(90deg, var(--accent-color-secondary-transparent-03), var(--accent-color));
        mask-size: cover;
        mask-position: center center;
        mask-repeat: no-repeat;
    }

    h1 {
        font-weight: 700;
    }

    h1, p {
        margin: 0;
    }

    >.center {
        position: absolute;
        width: 80%;
        top: calc(50%);
        left: 50%;
        transform: translate(-50%, -50%);
        display: grid;
        justify-content: center;
        align-items: center;
        text-align: center;
        //animation: ospfpodskfposdkfpsok 1s forwards ease;

        h1 {
            font-size: 128px;
            margin-bottom: 32px;
            font-weight: 1000;

            span {
                color: var(--accent-color-secondary);

                &:nth-child(1) {
                    color: var(--accent-color);
                }
            }
        }

        p {
            font-size: 24px;
            animation: aijdsjofj 5s infinite ease;
            text-align: center;
            margin: auto;
            overflow: hidden;
            white-space: nowrap;
            user-select: none;

            @keyframes aijdsjofj {
                0%, 20%, 100% {
                    width: 0;
                }
                70%, 90% {
                    width: 196px;
                }
            }
        }
    }
}

@media screen and (min-width: app.$mobileBreakpoint) and (max-width: app.$tabletBreakpoint){

}

@media screen and (max-width: app.$mobileBreakpoint) {
    section:is(.top) {
        height: calc(100svh);
        transition-duration: 0.3s;
    }
}
</style>