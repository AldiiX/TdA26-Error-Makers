<script setup lang="ts">
    import Menu from "~/components/Menu.vue";
    import { NuxtLink } from '#components';
</script>

<template>
    <footer :class="$style.footer">
        <div :class="$style.divider">
            <div :class="$style.top">
                <div :class="$style.left">
                    <div :class="$style.logo"></div>
                </div>
                <div :class="$style.right">
                        
                    <div :class="$style.navigation">
                        <p :class="$style.upper">Navigace</p>

                        <div style="gap: 8px; display: grid;">
                            <Menu :link-class="$style.link" />
                        </div>
                    </div>
                    
                    <div :class="$style.other">
                        <p :class="$style.upper">Ostatní</p>

                        <div style="gap: 8px; display: grid;">
                            <NuxtLink to="/other/gdpr" :class="$style.link">GDPR</NuxtLink>
                            <NuxtLink to="/other/cookies" :class="$style.link">Cookies</NuxtLink>
                            <NuxtLink to="/other/tos" :class="$style.link">ToS</NuxtLink>
                        </div>
                    </div>
                    
                    <div :class="$style.team">
                        <p :class="$style.upper">Tým</p>
                        <p>
                            Fullstack:&nbsp;&nbsp;&nbsp;&nbsp;
                            <a href="https://stanislavskudrna.cz" target="_blank"> Stanislav Škudrna</a>
                        </p>
                        <p>
                          Fullstack:&nbsp;&nbsp;&nbsp;&nbsp;
                          <a href="https://serhii.cz" target="_blank"> Serhii Yavorskyi</a>
                        </p>
                        <p>
                          Fullstack:&nbsp;&nbsp;&nbsp;&nbsp;
                          <a href="https://jakubsokol.cz" target="_blank"> Jakub Sokol</a>
                        </p>
                    </div>
                </div>
            </div>
            <div :class="$style.bottom">
                <p>© Think different Academy 2025-2026</p>
                <p>Vytvořeno týmem Error Makers ze školy <a href="https://educhem.cz" target="_blank">Střední škola EDUCHEM, a.s.</a> pro <a href="https://tourde.app/" target="_blank">Tour de App</a>.</p>
            </div>
        </div>
    </footer>
</template>

<style module lang="scss">

@use "../app" as app;

.footer {
    background-color: rgb(from var(--background-color-secondary) r g b / 0.2);
    display: flex;
    flex-direction: column;
    align-items: center;
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 0.6);
    width: 80%;
    margin: 48px auto;
    //margin-top: -300px; // debug
    padding: 32px 48px;
    border-radius: 48px;
    backdrop-filter: blur(10px)  saturate(1.2);
    font-family: Dosis, sans-serif;
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.6), 0 4px 30px rgba(0, 0, 0, 0.1);
    z-index: 5;

    .divider {
        width: 100%;

        .top {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-bottom: 48px;

            .left {
                display: flex;

                .logo {
                    mask-image: url(../../public/icons/Think-different-Academy_LOGO_textove-cerne.svg);
                    mask-size: contain;
                    mask-repeat: no-repeat;
                    mask-position: center;
                    height: 128px;
                    aspect-ratio: 2 / 1;
                    background: linear-gradient(90deg, var(--accent-color), var(--accent-color-secondary-theme));
                }
            }

            .right {
                display: flex;
                width:  60%;
                justify-content: space-between;
                
                .navigation,
                .other,
                .team {
                    .upper{
                        font-size: 24px;
                        font-weight: 700;
                        margin-bottom: 20px;
                        border-bottom: 1px solid;
                        width: fit-content;
                        border-bottom-color: var(--text-color-primary);
                        pointer-events: none;
                    }
                    
                    p {
                        margin: 8px 0;
                    }

                    a, .link {
                        color: var(--text-color-primary);
                        text-decoration: none;
                        transition-duration: 0.3s;

                        &:hover {
                            color: var(--accent-color-secondary-theme);
                            transition-duration: 0.3s;
                        }

                        &:is(:global(.router-link-active)) {
                            color: var(--accent-color-primary);
                        }
                    }

                    .link {
                        user-select: none;
                    }
                }
            }
        }

        .bottom {
            display:  flex;
            justify-content: space-between;
            border-top: 1px solid rgb(from var(--text-color-primary) r g b / 0.1);
            padding: 16px 0;
            padding-top: 48px;
            font-size: 14px;
            

            p {
                margin: 0;


                a {
                    color: var(--text-color-primary);
                    text-decoration: none;
                    font-size: 16px;
                    transition-duration: 0.3s;

                    &:hover{
                        color: var(--accent-color-secondary-theme);
                        transition-duration: 0.3s;
                    }
                }
            }
        }
    }
}

@media screen and (max-width:app.$mobileBreakpoint) {
    .footer{
        
        .divider {
            
            .top{
                display: flex;
                flex-direction: column;
                
                .left{
                    
                }
                
                .right{
                    display: flex;
                    flex-direction: column;
                    gap: 36px;
                    width: 100%;
                    
                    .navigation{
                        display: none;
                    }
                    
                    .other{
                        
                    }
                    
                    .team{
                        
                        .upper{
                            
                            p{
                            }                            
                        }
                    }
                }
            }
            
            .bottom{
                flex-direction: column;
                gap: 16px;
                text-align: center;
                
                p{
                    margin: 0;
                    font-size: 14px;
                }
                
                a{
                    color: var(--accent-color-secondary-theme) !important;
                    text-decoration: none;
                    font-size: 16px;
                    transition-duration: 0.3s;
                }
            }
        }
    }
}

@media screen and (max-width:app.$tabletBreakpoint) {
    .footer {
        .divider {
            
            .top {
                display: flex;
                flex-direction: column;
                gap: 16px;
                
                .right {
                    display: flex;
                    width: 100%;
                    gap: 24px;
                }
            }
        }
    }
}
</style>