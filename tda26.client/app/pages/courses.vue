<script setup lang="ts">
    import { Head, Title } from '#components';

    definePageMeta({
        layout: "normal-page-layout"
    });
</script>

<template>
    <Head>
        <Title>Kurzy • Think different Academy</Title>
    </Head>
    <section :class="$style.section">
        <div :class="$style.topContainer">
            <div :class="$style.left">
                <h1 :class="$style.nadpis">Kurzy</h1>
                <p :class="$style.podnapis">Objev naši širokou nabídku kurzů, které pokrývají různé oblasti technologií a programování.
                    Ať už jsi začátečník nebo pokročilý, máme pro tebe kurz, který ti pomůže rozvíjet tvé dovednosti a znalosti.
                </p>
            </div>
            <div :class="$style.right">
                <div :class="[$style.coursesInfo , 'liquid-glass']">
                    <div :class="$style.row">
                        <span :class="$style.number">152</span>
                        <span :class="$style.text">Kurzů</span>
                    </div>
                    <div :class="$style.row">
                        <span :class="$style.number">24</span>
                        <span :class="$style.text">Nových kurzů</span>
                    </div>
                </div>
            </div>
        </div>
        
        <div :class="$style.bottomContainer">
            <div :class="$style.left">
                <div :class="$style.filtersLeft">
                    <p>Filtry</p>
                    <div :class="[$style.searchBar, 'liquid-glass']">
                        <div :class="$style.searchIcon"></div>
                        <input type="text" placeholder="Hledat kurz..." />
                    </div>
                    <div :class="$style.sortOptions">

                    </div>
                </div>
            </div>
            
            <div :class="$style.right">
                <div :class="$style.filtersTop">
                    
                </div>
                <div :class="$style.courses">
                    <div :class="$style.coursesList">

                    </div>

                    <div :class="$style.pagination">

                    </div>
                </div>
            </div>
        </div>
    </section>
    
</template>

<style module lang="scss">
.section{
    display: flex;
    flex-direction: column;
    gap: 64px;
    
    .topContainer {
        display: flex;
        justify-content: space-between;

        .left {
            display: flex;
            flex-direction: column;
            justify-content: center;

            .nadpis {
                font-size: 64px;
                margin:0;
            }

            .podnapis {
                font-size: 20px;
                margin-top: 16px;
                max-width: 700px;
                color: var(--text-color-secondary);
            }

        }

        .right {
            display: flex;
            flex-direction: column;
            justify-content: center;
            margin-right: 256px;
            margin-top: 32px;
            min-width: 216px;

            .coursesInfo {
                display: flex;
                align-items: flex-start;
                justify-content: center;
                flex-direction: column;
                background: linear-gradient(135deg, var(--accent-color-secondary-darker), var(--accent-color-secondary)) ;
                border-radius: 48px;
                padding: 16px 32px;

                .row {
                    display: flex;
                    align-items: center;
                    gap: 8px;


                    .number, .text {
                        color: var(--text-color-primary);
                        font-size: 22px;
                        margin: 0;
                    }

                    .number{
                        font-size: 28px;
                    }
                }
            }
        }
    }

    .bottomContainer {
        display: flex;
        width: 100%;
        height: 90vh;
        gap: 64px;


        .left {
            display: flex;
            flex-direction: column;
            width: 256px;
            background-color: var(--background-color-secondary);
            border-radius: 16px;
            box-shadow: 12px 0 32px rgba(0, 0, 0, 0.1);

            .filtersLeft {
                
                p{
                    font-size: 36px;
                    font-weight: 700;
                    margin: 24px 24px;
                }
                .searchBar {
                    height: 48px;
                    background-color: var(--accent-color-secondary);
                    display: flex;
                    align-items: center;
                    justify-self: center;
                    gap: 10px;
                    width: 80%;
                    border-radius: 12px;
                    padding: 16px;
                    transition: all 0.2s ease-in-out;


                    .searchIcon {
                        mask-image: url('../../public/icons/search.svg');
                        mask-size: cover;
                        mask-position: center;
                        mask-repeat: no-repeat;

                        width: 24px;
                        height: 24px;
                        background-color: var(--text-color-secondary);
                        opacity: 0.8;
                    }

                    input {
                        width: 100%;
                        border: none;
                        outline: none;
                        font-size: 16px;
                        color: var(--text-color-secondary);
                        background: transparent;
                        font-family: 'Poppins', sans-serif;

                        &::placeholder {
                            color: var(--text-color-secondary);
                            opacity: 0.8;
                        }
                    }
                }

                .sortOptions {
                    margin-top: 32px;
                    height: calc(100% - 64px - 32px);
                    background-color: var(--background-color-secondary);
                }
            }
        }

        .right {
            display: flex;
            flex-direction: column;
            gap: 32px;
            min-width: 0;
            flex: 1 1 auto;
            
            .filtersTop {
                width: 100%;
                height: 64px;
                background-color: var(--background-color-secondary);
            }
            
            .courses{
                
                .coursesList {
                    height: calc(80vh - 64px - 32px);
                    background-color: var(--background-color-secondary);
                }

                .pagination {
                    height: 64px;
                    background-color: var(--accent-color-secondary-darker);
                }    
            }
        }
    }
}
    

</style>

