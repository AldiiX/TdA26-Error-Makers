<script setup lang="ts">

</script>

<template>
    <div :class="$style.bg">
        <div :class="$style.one"></div>
        <div :class="$style.two"></div>
    </div>
</template>

<style module lang="scss">
.bg {
    position: fixed;
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100vw;
    height: 100vh;
    filter: blur(20vw);
    opacity: 0.7;
    pointer-events: none;
    z-index: -1;
    transition-duration: 0.3s;
    transition-property: background-color;

    &:is(.disableanimations) {
        .one, .two {
            animation: none !important;
        }
    }

    :global(:root[data-theme='light']) & {
        filter: blur(10vw);

        .one, .two {
            width: 35vw;
        }
    }

    .one, .two {
        width: 23vw;
    }

    .one {
        position: absolute;
        aspect-ratio: 1/1;
        background-color: var(--accent-color-secondary-theme);
        bottom: -20vw;
        left: -10vw;
        border-radius: 100%;
        animation: blobanim 10s infinite ease;
    }

    .two {
        position: absolute;
        aspect-ratio: 1/1;
        background-color: var(--accent-color-primary);
        top: -20vw;
        right: -10vw;
        border-radius: 100%;
        animation: blobanim 15s infinite ease;
        animation-delay: 2.5s;
    }

    @keyframes blobanim {
        0%,100% {
            transform: scale(1);
        }
        50% {
            transform: scale(1.18);
        }
    }
}
</style>
