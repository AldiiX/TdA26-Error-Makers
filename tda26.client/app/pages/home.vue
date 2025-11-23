<script setup lang="ts">
    import Header from "~/components/Header.vue";
    import Footer from "~/components/Footer.vue";
    import Input from "~/components/Input.vue";
    import Button from "~/components/Button.vue";
    import { Head, Title, NuxtLink } from '#components';
    import TypeWriter from "~/components/TypeWriter.vue";
    import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
    import BlurText from "~/components/BlurText.vue";
    import CircleBlurBlob from "~/components/CircleBlurBlob.vue";

    const theme = useState("theme");

    definePageMeta({
        alias: "/"
    })
</script>




<template>
    <Head>
        <Title>Domů • Think different Academy</Title>
    </Head>

    <Header/>

    <CircleBlurBlob bottom="0vw" left="-5vw" blur="12vw" size="15vw" color="var(--accent-color-primary)" />

    <section :class="$style.top">
        <div :class="$style.blob"></div>

        <div :class="$style.center">
            <div :class="$style.left">
                <BlurText text="Objevuj kurzy, které tě posunou." tag="h1" />
                <p>Studium nemusí být jen o biflování. S našimi interaktivními kurzy se učení stává zábavou.</p>

<!--                <h1>Objevuj kurzy, které tě posunou.</h1>-->
<!--                <SmoothSizeWrapper>-->
<!--                    <TypeWriter element="p" text="Studium nemusí být jen o biflování. S našimi interaktivními kurzy se učení stává zábavou." :speed="50" />-->
<!--                </SmoothSizeWrapper>-->

                <div :class="$style.btns">
                    <NuxtLink to="/courses">
                        <Button :class="$style.btn" href="/courses" button-style="primary" >Všechny kurzy</Button>
                    </NuxtLink>

                    <NuxtLink to="/lecturers">
                        <Button :class="$style.btn" href="/lecturers" button-style="secondary">Lektoři</Button>
                    </NuxtLink>
                </div>

                <Input :class="$style.input" placeholder="Najdi kurz nebo lektora..." />
            </div>

            <div :class="$style.right">
                <div :class="$style.img"></div>
            </div>
        </div>
    </section>
    
    <Footer/>
</template>


<style module lang="scss">
@use "../app" as app;


.top {
    position: relative;
    height: calc(100vh + 80px);
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

        :global(:root[data-theme='dark']) & {
            opacity: 0.5;
        }
    }

    h1 {
        font-weight: 700;
    }

    h1, p {
        margin: 0;
    }

    >.center {
        animation: idsojfodsijf 0.8s forwards ease;
        position: absolute;
        width: 80%;
        top: calc(50% - 40px);
        left: 50%;
        transform: translate(-50%, -50%);
        display: flex;
        justify-content: space-between;
        align-items: center;

        @keyframes idsojfodsijf {
            from {
                opacity: 0;
                filter: blur(12px);
                transform: translate(-50%, calc(-50% + 64px));
            }

            to {
                opacity: 1;
                filter: blur(0px);
                transform: translate(-50%, -50%);
            }
        }



        >.left {
            h1 {
                width: clamp(0px, 50vw, 700px);
                font-size: clamp(0px, 6vw, 96px);
                line-height: clamp(0px, 5.5vw, 104px);
                position: relative;
                color: var(--text-color-primary);
            }

            p {
                width: 30vw;
                font-size: clamp(0px, 1.5vw, 24px);

                margin-top: 2vw;
                margin-bottom: 2vw;
                opacity: 0.7;
            }

            .input {
                font-size: clamp(0px, 1.5vw, 20px);
                width: 100%;
                padding: 20px;
            }

            >.btns {
                display: flex;
                gap: 1vw;
                margin-bottom: 1vw;
                color: unset;
                
                .btn{
                    font-size: clamp(0px, 1.5vw, 20px);
                    padding: 0.8vw 2vw;
                }
            }

        }
    }
}

@media screen and (min-width: app.$mobileBreakpoint) and (max-width: app.$tabletBreakpoint){
    section:is(.top) > .center > .left > p {
        width: 45vw;
        font-size: clamp(0px, 2.5vw, 24px);
        margin-top: 2vw;
        opacity: 0.7;
    }
}

@media screen and (max-width: app.$mobileBreakpoint) {
    section:is(.top) {
        height: calc(100svh + 80px);
        transition-duration: 0.3s;

        >.center {
            position: absolute;
            width: auto;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            display: flex;
            justify-content: space-between;
            align-items: center;

            >.left {
                h1{
                    width: 80vw;
                    font-size: 48px;
                    line-height: 48px;
                    position: relative;

                    
                }

                p {
                    width: 80vw;
                    font-size: 16px;
                    margin-top: 16px;
                    opacity: 0.7;
                }

                >button{
                    border-radius: 12px;
                    padding: 10px 30px;
                    font-size: 16px;
                    cursor: pointer;
                    margin: 0;
                    margin-top: 16px;
                }
            }
            >.right {
                display: none;
            }
        }
    }
}
</style>
