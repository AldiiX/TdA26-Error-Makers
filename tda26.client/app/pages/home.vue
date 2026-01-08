<script setup lang="ts">
    import Header from "~/components/Header.vue";
    import Footer from "~/components/Footer.vue";
    import Button from "~/components/Button.vue";
    import { NuxtLink } from '#components';
    import TypeWriter from "~/components/TypeWriter.vue";
    import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
    import BlurText from "~/components/BlurText.vue";
    import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
    import Input from "~/components/Input.vue";

    const theme = useState("theme");

    definePageMeta({
        alias: "/"
    })

    // SEO
    useSeo({
        title: "Domů",
        description: "Objevuj kurzy, které tě posunou. Studium nemusí být jen o biflování. S našimi interaktivními kurzy se učení stává zábavou.",
        keywords: "online kurzy, vzdělávání, e-learning, interaktivní kurzy, výuka, studium"
    });

    // hledani kurzu
    const searchQuery = ref<string>("");

    async function submitSearchQuery() {
        if (searchQuery.value.trim() === "") return;

        // presmerovani na stranku s vysledky hledani
        await navigateTo(`/courses?search=${encodeURIComponent(searchQuery.value.trim())}`);
    }
</script>




<template>
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
                        <Button :class="$style.btn" href="/courses" button-style="primary" accent-color="primary">Všechny kurzy</Button>
                    </NuxtLink>

                    <NuxtLink to="/lecturers">
                        <Button :class="$style.btn" href="/lecturers" button-style="secondary">Lektoři</Button>
                    </NuxtLink>
                </div>

                <div :class="$style.inputWrapper">
                    <Input :class="$style.input" placeholder="Najdi kurz podle názvu nebo popisu..." v-model="searchQuery" @keyup.enter="submitSearchQuery" />
                    <div :class="$style.submit" @click="submitSearchQuery"></div>
                </div>
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

            .inputWrapper {
                position: relative;

                .input {
                    font-size: clamp(0px, 1.5vw, 20px);
                    width: 100%;
                    padding: 20px;
                    padding-right: 64px;
                }

                .submit {
                    width: 32px;
                    height: 32px;
                    position: absolute;
                    top: 50%;
                    right: 16px;
                    transform: translateY(-50%);
                    mask-image: url('../../public/icons/search.svg');
                    mask-size: cover;
                    mask-position: center;
                    mask-repeat: no-repeat;
                    background-color: var(--text-color-secondary);
                    opacity: 0.5;
                    cursor: pointer;
                    transition-duration: 0.3s;

                    &:hover {
                        opacity: 0.8;
                        transition-duration: 0.3s;
                    }
                }
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
