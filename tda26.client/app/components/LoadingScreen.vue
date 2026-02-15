<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";

const isLoading = ref(true);
const theme = useState<string>('theme', () => 'light');

// Compute styles based on theme
const backgroundGradient = computed(() => {
    return theme.value === 'dark'
        ? 'linear-gradient(180deg, #1a1a1a 0%, #090b0d 100%)'
        : 'linear-gradient(180deg, #FBFDFF 0%, #E7F0FB 100%)';
});

const titleColor = computed(() => {
    return theme.value === 'dark' ? '#FFFFFF' : '#080808';
});

const textColor = computed(() => {
    return theme.value === 'dark' ? '#999999' : '#666666';
});

onMounted(() => {
    const startTime = Date.now();
    const minLoadingTime = 500;

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
                hideLoader();
            });
    } else {
        setTimeout(hideLoader, 1000);
    }
});
</script>

<template>
    <Transition name="fade-out">
        <div v-if="isLoading" :class="$style.loadingScreen" :style="{ background: backgroundGradient }">
            <CircleBlurBlob top="10vw" left="-10vw" blur="10vw" color="var(--accent-color-secondary-theme)" />
            <CircleBlurBlob bottom="10vw" right="-10vw" blur="10vw" color="var(--accent-color-primary)" />

            <div :class="$style.loaderContainer">
                <div :class="$style.iconWrapper">
                    <div :class="$style.icon"/>
                </div>
                <h1 :class="$style.loadingTitle" :style="{ color: titleColor }">Think Different Academy se načítá</h1>
                <p :class="$style.loadingText" :style="{ color: textColor }">... připrav se na úžasné vzdělávací zážitky! ...</p>
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
    -webkit-mask: url("/icons/zarivka_thinking_bile.svg") no-repeat center;
    -webkit-mask-size: contain;
    background: var(--text-color);
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
    font-weight: 800;
    margin: 32px auto 8px;
    text-align: center;
    max-width: 90vw;
}

.loadingText {
    font-family: 'Dosis', Arial, sans-serif;
    font-size: 16px;
    font-weight: 500;
    margin: 0 auto;
    text-align: center;
    max-width: 90vw;
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
