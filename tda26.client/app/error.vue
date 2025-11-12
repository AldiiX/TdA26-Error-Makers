<script setup lang="ts">
import type {WebTheme} from "#shared/types";
import Header from "~/components/Header.vue";
import Footer from "~/components/Footer.vue";
import BlurBackground from "~/components/backgrounds/BlurBackground.vue";
import Button from "~/components/Button.vue";

const nuxtError = useError();
const theme = useState<WebTheme>('theme', () => 'light');


useHead({
    htmlAttrs: { 'data-theme': computed(() => theme.value ?? undefined) },
    link: [
        {
            rel: 'icon',
            type: 'image/x-icon',
            href: '/favicon-error.ico'
        }
    ],
});

const code = computed<number>(() => Number(nuxtError.value?.statusCode ?? 500))

const message = computed<string>(() => {
    const c = code.value
    if(nuxtError.value?.statusMessage || nuxtError.value?.message) {
        return (nuxtError.value?.statusMessage || nuxtError.value?.message) as string
    }

    switch (c) {
        case 404: return 'Stránka nebyla nalezena';
        case 403: return 'Nemáte dostatečná oprávnění';
        case 500: return 'Něco se pokazilo';
        default:  return (nuxtError.value?.statusMessage || nuxtError.value?.message) ?? 'Unexpected error';
    }
})

const splitNumber = computed<string[]>(() => {
    const str = code.value.toString().padStart(3, '0')
    return str.split('')
})



// nastav titulek stranky reaktivne
useHead(() => ({
    title: `Error ${code.value} • Think different Academy`,
}))

function goHome() {
    clearError({ redirect: '/' })
}

function goBack() {
    window.history.back();
}

function reloadPage() {
    window.location.reload();
}
</script>

<template>
    <Header/>

    <BlurBackground/>

    <main :class="$style.main">
        <div :class="$style.center">
            <div :class="$style.errorCode">
                
            </div>
            <div :class="$style.codeContainer"> 
                <p :class="$style.firstNumber">{{ splitNumber[0] ?? '' }}</p>


                <p :class="$style.firstNumber" v-if="splitNumber[1] !== '0'">{{ splitNumber[1] }}</p>
                <div :class="$style.icon" v-else></div>

                <p :class="$style.thirdNumber"> {{ splitNumber[2] ?? '' }}</p>
            </div>
            <p :class="$style.desc">{{ message }}</p>
            <div :class="$style.buttons">
                <Button :class="$style.btn" @click="goBack" button-style="primary" style="display: grid; font-size: 22px;" >Zpět</Button>
                <Button :class="$style.btn" @click="goHome" button-style="secondary" style="display: grid; font-size: 22px;" >Domů</Button>
            </div>
        </div>
    </main>

    <Footer/>
</template>



<style module lang="scss">
.main{
    width: 80%;
    height: calc(100vh);
    margin: 0 auto;
    position: relative;
    
    .center{
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        
        
        .codeContainer{
            $gradientAngle: 135deg;

            background: linear-gradient($gradientAngle, var(--accent-color), var(--accent-color-secondary-theme));
            -webkit-background-clip: text;
            background-clip: text;
            -webkit-text-fill-color: transparent;
            color: transparent;

            display: flex;
            align-items: center;
            justify-content: center;
            gap: 64px;
            
            
            
            .firstNumber,
            .icon,
            .thirdNumber {
                font-size: 264px;
                font-weight: 700;
                font-family: 'Dosis', sans-serif;
                margin: 0;
                display: inline-block;
                color: inherit; /* get clipped gradient from parent */
                -webkit-text-fill-color: transparent;
            }
            
            .icon {
                mask-image: url(../public/icons/zarivka_sad_bile.svg);
                mask-size: contain;
                mask-repeat: no-repeat;
                mask-position: center;
                height: 256px;
                aspect-ratio: 1 / 1;
                background: linear-gradient($gradientAngle, var(--accent-color), var(--accent-color-secondary-theme));
                animation: float 3s ease-in-out infinite alternate;

                @keyframes float {
                    0% {
                        transform: translateY(0);
                    }
                    50% {
                        transform: translateY(20px);
                    }
                    100% {
                        transform: translateY(0);
                    }
                }
            }
        }
        
        .desc{
            display: flex;
            justify-self: center;
            font-size: 32px;
            font-weight: 800;
            margin: 16px 0 32px 0;
        }
        
        .buttons{
            gap: 24px;
            display: flex;
            justify-content: center;

            .btn{
                flex: 1 1 0;
                min-width: 96px;
                max-width: 164px;
                display: inline-grid; 
                place-items: center;
                font-size: 22px;
            }
        }
    }
}


</style>