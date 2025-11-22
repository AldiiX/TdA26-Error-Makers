<script setup lang="ts">
    import { onMounted, onUnmounted, inject, type Ref } from 'vue';
    import { NuxtLink } from '#components';
    import Button from "~/components/Button.vue";
    import Menu from "~/components/Menu.vue";
    import type {Account} from "#shared/types";
    import Avatar from "~/components/Avatar.vue";
    
    const $style = useCssModule();
    
    const theme = useCookie('theme')

    const loggedAccount = useState<Account | null>('loggedAccount', () => null);

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
            <NuxtLink to="/" custom v-slot="{ navigate, href, isActive, isExactActive }">
                <div :class="[$style['Header-Logo'], { active: $style.isActive }]" @click="navigate"></div>
            </NuxtLink>

            <div :class="[$style['Header-Menu'], { locked: $style.isTransitioning }]">
                <Menu :link-class="$style.link" />

                <div :class="$style.btns" v-if="!loggedAccount">
                    <NuxtLink to="/login">
                        <Button button-style="primary" href="/login" accent-color="primary">Přihlásit se</Button>
                    </NuxtLink>

                    <NuxtLink to="/register">
                        <Button button-style="primary" href="/register" accent-color="secondary">Registrovat se</Button>
                    </NuxtLink>
                </div>

                <template v-else>
                    <div :class="$style.btns">
                        <NuxtLink :class="$style.linkBtn" to="/dashboard">
                            <Button button-style="primary" href="/dashboard">Dashboard</Button>
                        </NuxtLink>
                    </div>

                    <div :class="$style.loggedAs">
                        <div>
                            <p>Přihlášen jako</p>
                            <p :id="$style.accountName" :class="[$style.name, 'text-gradient']">{{ loggedAccount.firstName }} {{ loggedAccount.lastName }}</p>
                            <p :class="[$style.name, $style.shadow]">{{ loggedAccount.firstName }} {{ loggedAccount.lastName }}A</p>
                        </div>

                        <Avatar :name="loggedAccount.firstName + ' ' + loggedAccount.lastName" :src="loggedAccount.pictureUrl" :size="48" />
                    </div>
                    <div :class="$style.popoverContainer">
                        <div :class="$style.accountInfo">
                            <div :class="$style.accountDetails"> 
                                <Avatar :name="loggedAccount.firstName + ' ' + loggedAccount.lastName" :src="loggedAccount.pictureUrl" :size="128" />
                                <div :class="$style.accountText">
                                    <p :class="$style.name">{{ loggedAccount.firstName }} {{ loggedAccount.lastName }}</p>
                                    <p>{{ loggedAccount.email }}</p>
                                </div>
                                <div :class="$style.btns">
                                    <div :class="[$style.button, $style.logoutBtn]">
                                        <p :class="$style.container">
                                            <div :class="$style.icon"></div>
                                            <p>Odhlásit se</p>
                                        </p>
                                    </div>
                                    <div :class="[$style.button, $style.themeBtn]">
                                        <div :class="$style.container">
                                            <div :class="$style.icon"></div>
                                            <p>Přepnout téma</p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </template>
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
        backdrop-filter: blur(8px) saturate(1.2);
        transition-duration: 0.3s;

        /*#accountName {
            background: transparent;
            color: var(--text-color-primary);
            background-clip: unset;
            -webkit-text-fill-color: unset;
        }*/
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
                
                >.linkBtn {
                    
                    button{
                        text-decoration: none;
                        color: var(--text-color-primary);
                    }
                }
            }

            .loggedAs {
                display: flex;
                align-items: center;
                gap: 12px;
                position: relative;

                >div {
                    display: grid;

                    p {
                        margin: 0;
                        text-align: right;

                        &:is(.name) {
                            font-weight: 700;
                            font-size: 20px;
                        }

                        &:is(.shadow) {
                            position: absolute;
                            bottom: 2px;
                            left: 0px;
                            z-index: -1;
                            color: transparent;
                            text-shadow: 0 0 24px var(--background-color), 0px 0 12px var(--background-color);
                        }
                    }
                }

                &:hover .popoverContainer{
                    display: flex;
                }
            }
            
            .popoverContainer{
                position: absolute;
                display: none;
                top: 70px;
                right: 0;
                background-color: var(--background-color-secondary);
                border-radius: 16px;
                padding: 16px;
                width: 256px;
                z-index: 10;
                
                
                >.accountInfo{
                    
                    >.accountDetails{
                        
                        div{
                            
                        }
                        
                        >.accountText{
                            
                        }
                        
                        >.btns{
                            
                            >.button{
                                
                            }
                        }
                    }
                }
            }

            &:is(.locked) {
                pointer-events: none;
            }

            >.link {
                position: relative;
                text-decoration: none;
                font-weight: 1000;
                color: var(--text-color-primary);
                user-select: none;
                transition-duration: 0.3s;

                &:is(:global(.router-link-active)) {
                    pointer-events: none;
                    //color: var(--accent-color-primary);

                    &::after {
                        content: "";
                        position: absolute;
                        left: 0;
                        bottom: -4px;
                        width: 100%;
                        height: 2px;
                        background-color: var(--accent-color-primary);
                        transform: scaleX(1);
                        transition: transform 0.3s ease;
                        animation: oksfodsk 0.3s forwards ease;
                        transform-origin: left;

                        @keyframes oksfodsk {
                            from {
                                transform: scaleX(0);
                            }
                            to {
                                transform: scaleX(1);
                            }
                        }
                    }
                }

                &::after {
                    content: "";
                    position: absolute;
                    left: 0;
                    bottom: -4px;
                    width: 100%;
                    height: 2px;
                    background-color: var(--accent-color-secondary-theme);
                    transform: scaleX(0);
                    transform-origin: right;
                    transition: transform 0.3s ease;
                }

                &:hover::after {
                    transform: scaleX(1);
                    transform-origin: left;
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
        //mask-image: url(../../public/images/svg/menu.svg);
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
