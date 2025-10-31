<script setup lang="ts">
    import { ref, onMounted, onUnmounted } from "vue";
    import BlurBackground from "@/components/backgrounds/BlurBackground.vue";
    import ColoredBackground from "@/components/backgrounds/ColoredBackground.vue";
    import { RouterLink } from "vue-router";
    import Header from "~/components/Header.vue";
    import Footer from "~/components/Footer.vue";
    import Input from "~/components/Input.vue";
    import Button from "~/components/Button.vue";
    import { Head, Title } from '#components';

    const theme = useState("theme");

onMounted(() => {
    fetch("/api/v1").then(async response => {
        return response.json();
    })
});
</script>




<template>
    <Head>
        <Title>Domů • Think different Academy</Title>
    </Head>

    <Header/>

    <section :class="$style.top">
        <div :class="$style.blob" :style="{ opacity: (theme == 'dark' ? '0.5' : '') }"></div>

        <div :class="$style.center">
            <div :class="$style.left">
                <h1>Objevuj kurzy, které tě posunou.</h1>
                <p>Studium nemusí být jen o biflování. S našimi interaktivními kurzy se učení stává zábavou.</p>
                <div :class="$style.btns">
                    <Button :class="$style.btn" href="/courses" button-style="primary" >Všechny kurzy</Button>
                    <Button :class="$style.btn" href="/lecturers" button-style="secondary">Lektoři</Button>
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

@keyframes ospfpodskfposdkfpsok {
    from { opacity: 0; margin-top: 100px; }
    to { opacity: 1; margin-top: 0;}
}

section:is(.top) {
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
        top: calc(50% - 40px);
        left: 50%;
        transform: translate(-50%, -50%);
        display: flex;
        justify-content: space-between;
        align-items: center;
        //animation: ospfpodskfposdkfpsok 1s forwards ease;



        >.left {
            >h1 {
                width: clamp(0px, 50vw, 700px);
                font-size: clamp(0px, 6vw, 96px);
                line-height: clamp(0px, 5.5vw, 104px);
                position: relative;
                color: var(--text-color-primary);

                //&::before {
                //    content: 'X O O X X O';
                //    position: absolute;
                //    bottom: 100%;
                //    left: 0;
                //    font-size: clamp(0px, 1.5vw, 32px);
                //    color: #ff7575;
                //}
            }

            >p {
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

                >.btn{
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
            //animation: ospfpodskfposdkfpsok 1s forwards ease;

            >.left {
                h1{
                    width: 80vw;
                    font-size: 48px;
                    line-height: 48px;
                    position: relative;

                    //&::before {
                    //    content: "X O O X X O";
                    //    position: absolute;
                    //    bottom: 100%;
                    //    left: 0;
                    //    font-size: 16px;
                    //    color: #ff7575;
                    //}
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
