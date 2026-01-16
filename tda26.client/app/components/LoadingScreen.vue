<script setup lang="ts">
import { ref, onMounted } from 'vue';

const isLoading = ref(true);

onMounted(() => {
    // Wait for fonts and critical resources to load
    if (document.fonts) {
        document.fonts.ready.then(() => {
            // Add a small delay to ensure all styles are applied
            setTimeout(() => {
                isLoading.value = false;
            }, 100);
        });
    } else {
        // Fallback for browsers without Font Loading API
        setTimeout(() => {
            isLoading.value = false;
        }, 1000);
    }
});
</script>

<template>
    <Transition name="fade-out">
        <div v-if="isLoading" :class="$style.loadingScreen">
            <div :class="$style.loaderContainer">
                <div :class="$style.loader">
                    <div :class="$style.circle"></div>
                    <div :class="$style.circle"></div>
                    <div :class="$style.circle"></div>
                </div>
                <p :class="$style.loadingText">Think Different Academy</p>
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
    background: linear-gradient(180deg, #FBFDFF 0%, #E7F0FB 100%);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999999;
}

.loaderContainer {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 24px;
}

.loader {
    display: flex;
    gap: 12px;
    align-items: center;
}

.circle {
    width: 16px;
    height: 16px;
    border-radius: 50%;
    background: linear-gradient(135deg, #0070BB 0%, #91F5AD 100%);
    animation: bounce 1.4s infinite ease-in-out both;
    
    &:nth-child(1) {
        animation-delay: -0.32s;
    }
    
    &:nth-child(2) {
        animation-delay: -0.16s;
    }
    
    &:nth-child(3) {
        animation-delay: 0s;
    }
}

@keyframes bounce {
    0%, 80%, 100% {
        transform: scale(0.6) translateY(0);
        opacity: 0.5;
    }
    40% {
        transform: scale(1) translateY(-20px);
        opacity: 1;
    }
}

.loadingText {
    font-family: 'Dosis', Arial, sans-serif;
    font-size: 20px;
    font-weight: 600;
    color: #080808;
    margin: 0;
    background: linear-gradient(90deg, #0070BB 0%, #55c374 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    animation: pulse 2s ease-in-out infinite;
}

@keyframes pulse {
    0%, 100% {
        opacity: 0.7;
    }
    50% {
        opacity: 1;
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
