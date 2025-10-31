<script setup lang="ts">
import type {WebTheme} from "~/lib/types";
import Header from "~/components/Header.vue";
import Footer from "~/components/Footer.vue";
import BlurBackground from "~/components/backgrounds/BlurBackground.vue";

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

if (import.meta.client && theme.value === null) {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
    theme.value = prefersDark ? 'dark' : 'light'
}

if (import.meta.client) {
    const mql = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = (e: MediaQueryListEvent) => {
        const hasCookie = useCookie<WebTheme>('theme').value
        if (!hasCookie) {
            theme.value = e.matches ? 'dark' : 'light'
        }
    }
    mql.addEventListener?.('change', handler)
}

const code = computed<number>(() => Number(nuxtError.value?.statusCode ?? 500))

const message = computed<string>(() => {
    const c = code.value
    // preferuj statusMessage/message z chyby, pokud neni jedna z nasich preset kategorii
    if (!([404, 403, 500] as number[]).includes(c)) {
        return (nuxtError.value?.statusMessage || nuxtError.value?.message) ?? 'Unexpected error'
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
    title: `Error ${code.value}`
}))

function goHome() {
    // zrusit chybu a presmerovat domu (oficialni cesta v nuxtu)
    clearError({ redirect: '/' }) // :contentReference[oaicite:1]{index=1}
}

function reloadPage() {
    window.location.reload();
}
</script>

<template>
    <Header/>
    <BlurBackground/>
    <main id="error-page" >
        <div :class="$style.center">
            <div :class="$style.errorCode">
                
            </div>
            <div :class="$style.codeContainer"> 
                <p :class="$style.firstNumber">{{ splitNumber[0] ?? '' }}</p>
                <div :class="$style.secondNumber"></div>
                <p :class="$style.thirdNumber"> {{ splitNumber[2] ?? '' }}</p>
            </div>
            <p :class="$style.desc">{{ message }}</p>
            <div :class="$style.buttons">
                <Button :class="$style.btn" @click="goHome" button-style="primary" style="display: grid; font-size: 22px;" >Zpět</Button>
                <Button :class="$style.btn" @click="reloadPage" button-style="secondary" style="display: grid; font-size: 22px;" >Načíst znovu</Button>
            </div>
        </div>
    </main>
    <Footer/>
</template>

<style module lang="scss">
main{
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
            background: linear-gradient(90deg, var(--accent-color), var(--accent-color-secondary-darker));
            -webkit-background-clip: text;
            background-clip: text;
            -webkit-text-fill-color: transparent;
            color: transparent;

            display: flex;
            align-items: center;
            justify-content: center;
            gap: 64px;
            
            
            
            .firstNumber,
            .secondNumber,
            .thirdNumber {
                font-size: 256px;
                font-weight: 700;
                font-family: 'Dosis', sans-serif;
                margin: 0;
                display: inline-block;
                color: inherit; /* get clipped gradient from parent */
                -webkit-text-fill-color: transparent;
            }
            
            .secondNumber{
                mask-image: url(../public/icons/zarivka_sad_bile.svg);
                mask-size: contain;
                mask-repeat: no-repeat;
                mask-position: center;
                height: 256px;
                aspect-ratio: 1 / 1;
                background: linear-gradient(90deg, var(--accent-color), var(--accent-color-secondary-darker));
            }
            
        }
        
        .desc{
            display: flex;
            justify-self: center;
            font-size: 32px;
            font-weight: 500;
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