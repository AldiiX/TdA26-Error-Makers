<script setup lang="ts">
    import { onMounted, onUnmounted, inject, type Ref } from 'vue';
    import { RouterLink } from 'vue-router';
    import Button from "~/components/Button.vue";
    const $style = useCssModule();

    function ouasihfdusifhi() {
        const header = document.querySelector("header");
        if(!header) return;

        if(window.scrollY > 0) {
            header.classList.add($style.scrolled)
        } else {
            header.classList.remove($style.scrolled)
        }
    }

    onMounted(() => {
        document.addEventListener("scroll", ouasihfdusifhi);
        ouasihfdusifhi();
    });

    onUnmounted(() => {
        document.removeEventListener("scroll", ouasihfdusifhi);
    });
</script>





<template>
    <header :class="$style.header">

        <div :class="$style.flex">
            <RouterLink to="/" custom v-slot="{ navigate, href, isActive, isExactActive }">
                <div :class="[$style['Header-Logo'], { active: $style.isActive }]" @click="navigate"></div>
            </RouterLink>

            <div :class="[$style['Header-Menu'], { locked: $style.isTransitioning }]">
                <RouterLink to="/">Domů</RouterLink>
                <RouterLink to="/courses">Kurzy</RouterLink>
                <RouterLink to="/lecturers">Lektoři</RouterLink>
                <RouterLink to="/about">O nás</RouterLink>
                <RouterLink to="/faq">FAQ</RouterLink>

                <div :class="$style.btns">
                    <Button button-style="primary" href="/account">Přihlásit se</Button>
                    <Button button-style="primary" href="/account" accent-color="var(--accent-color-secondary-darker)">Registrovat se</Button>
                </div>
            </div>

        </div>

        <!-- Menu pro mobily a tablety -->
        <div class="smallDevice"></div>
        <div class="Header-Logo-Small" onclick="location.href='/'"></div>
    </header>
</template>

<style module lang="scss">
.header {
    position: fixed;
    background: none;
    width: 88.5%;
    height: 80px;
    margin-top: 2.5vh;
    left: 50%;
    transform: translateX(-50%);
    top: 0;
    z-index: 5;
    transition-duration: 0.3s;
    border: 1px solid transparent;
    border-radius: 32px;

    &:is(.scrolled) {
        width: 80%;
        box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.6), 0 4px 30px rgba(0, 0, 0, 0.15);
        background-color: rgb(from var(--background-color-secondary) r g b / 0.3);
        border: 1px solid rgb(from var(--background-color-secondary) r g b / 0.6);
        backdrop-filter: blur(8px);
        transition-duration: 0.3s;
    }

    >.flex {
        position: relative;
        display: flex;
        height: 100%;
        width: 90%;
        align-items: center;
        justify-content: space-between;
        margin: 0 auto;

        >.Header-Logo {
            height: 50px;
            aspect-ratio: 150/47;
            display: flex;
            background-image: var(--footer-tda-logo);
            background-size: contain;
            background-repeat: no-repeat;
            background-position: center;
            cursor: pointer;
            transition-duration: 0.3s;
        }

        >.Header-Menu {
            position: absolute;
            display: flex;
            gap: 40px;
            align-items: center;
            justify-content: center;
            height: 100%;
            width: fit-content;
            top: 50%;
            right: 0;
            transform: translateY(-50%);

            .btns {
                display: flex;
                gap: 16px;
            }

            &:is(.locked) {
                pointer-events: none;
            }

            >a {
                position: relative;
                text-decoration: none;
                font-weight: 1000;
                color: var(--text-color-primary);
                user-select: none;
                transition-duration: 0.3s;

                &:is(.active), &:is(.router-link-active) {
                    pointer-events: none;

                    &::before {
                        content: "";
                        display: block;
                        width: 100%;
                        height: 2px;
                        background: var(--accent-color-primary);
                        margin-top: 22.5px;
                        position: absolute;
                        border-radius: 10px;
                        pointer-events: none;
                        transition-duration: 0.3s;

                        animation: asdfsdsadf 0.3s forwards ease-in-out;

                        @keyframes asdfsdsadf {
                            from { width: 0 }
                            to {  width: 100% }
                        }
                    }
                }

                &:hover {
                    &::after {
                        content: "";
                        display: block;
                        width: 100%;
                        height: 2px;
                        background: var(--accent-color-secondary);
                        margin-top: 5px;
                        position: absolute;
                        border-radius: 10px;
                        animation: asdfsdf 0.3s forwards ease-in-out;
                        pointer-events: none;
                        transition-duration: 0.3s;

                        @keyframes asdfsdf {
                            from { width: 0 }
                            to {  width: 100% }
                        }
                    }
                }
            }
        }
    }

    >.smallDevice{
        display: none;
        width: 50px;
        height: 50px;
        position: absolute;
        right: 5%;
        top: 50%;
        transform: translateY(-50%);
        mask-image: url(../../public/images/svg/menu.svg);
        mask-size: 90%;
        mask-position: center;
        mask-repeat: no-repeat;
        background-color: var(--text-color);
        cursor: pointer;
        transition-duration: 0.3s;

        &:hover {
            background-color: var(--color-gray) !important;
            transition-duration: 0.3s;
        }
    }

    >.Header-Logo-Small {
        display: none;
        height: 50px;
        width: 50px;
        mask-image: var(--tda-logo-small);
        mask-size: contain;
        mask-repeat: no-repeat;
        mask-position: center;
        background-color: var(--text-color);
        top: 50%;
        transform: translateY(-50%);
        position: absolute;
        left: 5%;
        cursor: pointer;
    }
}

@media screen and (min-width: 600px) and (max-width: 960px) {
    .header{
        >.flex {
            display: none;
        }
        >.smallDevice {
            display: unset;
        }
        >.Header-Logo-Small {
            display: grid;
        }
    }
}

@media screen and (max-width: 600px) {
    header{

        >.flex {
            display: none;
        }
        >.smallDevice {
            display: unset;
        }
        >.Header-Logo-Small {
            display: grid;
        }
    }
}
</style>
