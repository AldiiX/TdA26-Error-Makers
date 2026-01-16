<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';

const isLoading = ref(true);
const theme = useState<string>('theme', () => 'light');

// Compute background gradient based on theme
const backgroundGradient = computed(() => {
    return theme.value === 'dark'
        ? 'linear-gradient(180deg, #1a1a1a 0%, #090b0d 100%)'
        : 'linear-gradient(180deg, #FBFDFF 0%, #E7F0FB 100%)';
});

onMounted(() => {
    const startTime = Date.now();
    const minLoadingTime = 500; // Minimum time to show loading screen (in ms)
    
    // Wait for fonts and critical resources to load
    const hideLoader = () => {
        const elapsedTime = Date.now() - startTime;
        const remainingTime = Math.max(0, minLoadingTime - elapsedTime);
        
        setTimeout(() => {
            isLoading.value = false;
        }, remainingTime);
    };
    
    if (document.fonts) {
        document.fonts.ready
            .then(hideLoader)
            .catch(() => {
                // Fallback if font loading fails
                hideLoader();
            });
    } else {
        // Fallback for browsers without Font Loading API
        setTimeout(hideLoader, 1000);
    }
});
</script>

<template>
    <Transition name="fade-out">
        <div v-if="isLoading" :class="$style.loadingScreen" :style="{ background: backgroundGradient }">
            <div :class="$style.loaderContainer">
                <div :class="$style.iconWrapper">
                    <div :class="$style.icon"></div>
                </div>
                <h1 :class="$style.loadingTitle">Think Different Academy se načítá</h1>
                <p :class="$style.loadingText">... připravte se na úžasné vzdělávací zážitky! ...</p>
            </div>
        </div>
    </Transition>
</template>

<style module lang="scss">
.loadingScreen {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 10000;
}

.loaderContainer {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 24px;
    padding: 64px;
    border-radius: 12px;
    min-width: 600px;
    max-width: 90vw;
}

.iconWrapper {
    width: 250px;
    max-width: 90vw;
    aspect-ratio: 1/1;
    display: flex;
    justify-content: center;
    align-items: center;
}

.icon {
    width: 100%;
    height: 100%;
    mask: url("/icons/zarivka_sad_bile.svg") no-repeat center;
    mask-size: contain;
    -webkit-mask: url("/icons/zarivka_sad_bile.svg") no-repeat center;
    -webkit-mask-size: contain;
    background: linear-gradient(135deg, #0070BB 0%, #91F5AD 100%);
    animation: loading-icon-swing 3s ease-in-out infinite;
}

@keyframes loading-icon-swing {
    0% {
        transform: rotate(0deg);
    }
    20% {
        transform: rotate(-20deg);
    }
    60% {
        transform: rotate(20deg);
    }
    100% {
        transform: rotate(0deg);
    }
}

.loadingTitle {
    font-family: 'Dosis', Arial, sans-serif;
    font-size: 32px;
    font-weight: 700;
    margin: 32px auto 8px;
    text-align: center;
    color: #080808;
    max-width: 90vw;
    
    @media (prefers-color-scheme: dark) {
        color: #FFFFFF;
    }
}

.loadingText {
    font-family: 'Dosis', Arial, sans-serif;
    font-size: 16px;
    font-weight: 500;
    margin: 0 auto;
    text-align: center;
    color: #666666;
    max-width: 90vw;
    
    @media (prefers-color-scheme: dark) {
        color: #999999;
    }
}
</style>

<style>
/* Global fade-out transition */
.fade-out-enter-active,
.fade-out-leave-active {
    transition: opacity 0.5s ease;
}

.fade-out-enter-from,
.fade-out-leave-to {
    opacity: 0;
}
</style>
