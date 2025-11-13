<script setup lang="ts">
    import { Head, Title } from '#components';
    import CourseCard from '~/components/pagespecific/CourseCard.vue';
    import type {Course, Lecturer} from '#shared/types';
    import NumberExponential from "~/components/NumberExponential.vue";
    import getBaseUrl from "#shared/utils/getBaseUrl";
    import Blob from "~/components/Blob.vue";
    
    definePageMeta({
        layout: "normal-page-layout"
    });

    const { data: _courses, pending, error, refresh } = await useFetch<Course[]>(getBaseUrl() + '/api/v2/courses');
    const courses = computed(() => _courses.value ?? []);


    // TODO: pagination, filtering, sorting will be added later
    const page = ref(1);
    const PAGE_SIZE = 8;

    const paginatedCourses = computed(() => {
        if (!courses.value) return [];
        const start = (page.value - 1) * PAGE_SIZE;
        return courses.value.slice(start, start + PAGE_SIZE);
    });

    const totalPages = computed(() => {
        if (!courses.value) return 0;
        return Math.ceil(courses.value.length / PAGE_SIZE);
    });

    const goToPage = (newPage: number) => {
        if (newPage < 1 || newPage > totalPages.value) return;

        scrollTo({ top: window.innerHeight * 0.2, behavior: 'smooth' });
        page.value = newPage;
    };

    const goToNextPage = () => {
        goToPage(page.value + 1);
    };

    const goToLastPage = () => {
        goToPage(page.value - 1);
    };
    
</script>

<template>
    <Head>
        <Title>Kurzy • Think different Academy</Title>
    </Head>
    
    <Teleport to="#teleports">
        <div :class="$style.blob"></div>
    </Teleport>
    
    <section :class="$style.section">
        
        <div :class="$style.topContainer">
            <div :class="$style.left">
                <h1 :class="$style.nadpis">Kurzy</h1>
                <p :class="$style.podnapis">Objev naši širokou nabídku kurzů, které pokrývají různé oblasti technologií a programování.
                    Ať už jsi začátečník nebo pokročilý, máme pro tebe kurz, který ti pomůže rozvíjet tvé dovednosti a znalosti.
                </p>
            </div>

            <div :class="$style.right">
                <div :class="$style.coursesInfo">
                    <div :class="$style.row">
                        <NumberExponential :value="courses?.length ?? 0" :decimals="0" :duration-ms="1500" :locale="'cs-CZ'" :format-options="{ useGrouping: true }" :number-class="$style.number"/>
<!--                        <p :class="$style.number">{{ courses?.length }}</p>-->
                        <p :class="$style.text">Kurzů</p>
                    </div>
                    <div :class="$style.row">
                        <p :class="$style.number">5</p>
                        <p :class="$style.text">Nových kurzů</p>
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
                    <p>Seřadit: </p>
                    <div :class="$style.sortOptionsList">
                        <button type="button" :class="$style.sortOption">Nejnovější</button>
                        <button type="button" :class="$style.sortOption">Nejstarší</button>
                        <button type="button" :class="$style.sortOption">Nejlepe hodnocení</button>
                        <button type="button" :class="$style.sortOption">Nejvíce zhlédnutí</button>
                    </div>
                </div>

                <div :class="$style.courses">
                    <div :class="$style.coursesList">
                        <CourseCard
                            v-for="course in paginatedCourses"
                            :course="course"
                            :key="course.uuid"
                        />
                    </div>
                    <div :class="$style.pagination">
                        <!-- Pagination controls will go here -->
                        <p @click="goToLastPage()">zpet</p>
                        <p>Stránka {{ page }} z {{ totalPages }}</p>
                        <p @click="goToNextPage()">dopredu</p>
                    </div>
                </div>
            </div>
        </div>
    </section>
    
</template>

<style module lang="scss">

.blob{
    mask-image: url("../../public/icons/blob_curses1.svg");
    mask-size: 100vw;
    mask-position: top;
    mask-repeat: no-repeat;
    width: 100vw;
    aspect-ratio: 16/9;
    background: linear-gradient(60deg, var(--accent-color-secondary-transparent-03), var(--accent-color));
    position: absolute;
    top: -20vh;
    z-index: -1;
}
.section{
    display: flex;
    flex-direction: column;
    gap: 64px;
    
    .topContainer {
        display: flex;
        justify-content: space-between;
        width: 100%;
        gap: 32px;

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
            width: 30%;

            .coursesInfo {
                display: flex;
                align-items: flex-start;
                justify-content: center;
                flex-direction: column;
                background: linear-gradient(135deg, var(--accent-color-secondary-darker), var(--accent-color-secondary)) ;
                border-radius: 48px;
                padding: 16px 32px;
                width: fit-content;

                .row {
                    display: flex;
                    align-items: center;
                    gap: 8px;


                    .number, .text {
                        color: var(--accent-color-secondary-theme-text);
                        font-size: 22px;
                        margin: 0;
                    }

                    .number {
                        font-size: 28px;
                    }
                }
            }
        }
    }

    .bottomContainer {
        display: flex;
        width: 100%;
        min-height: 90vh;
        gap: 64px;


        .left {
            display: flex;
            flex-direction: column;
            width: 256px;
            height: 100vh;
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
                        font-size: 18px;
                        color: var(--text-color-secondary);
                        background: transparent;
                        font-family: 'Dosis', sans-serif;

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
                display: flex;
                align-items: center;
                gap: 16px;
                padding: 0 16px;
                width: 100%;
                height: 64px;
                border-radius: 16px;
                background-color: var(--background-color-secondary);
                box-shadow: 12px 0 32px rgba(0, 0, 0, 0.1);
                
                >p {
                    font-size: 18px;
                    color: var(--text-color-secondary);
                    margin-right: 48px;
                }

                .sortOptionsList{
                    display: flex;
                    gap: 12px;
                    align-items: center;

                    .sortOption {
                        appearance: none;
                        border: none;
                        background: transparent;
                        padding: 8px 12px;
                        border-radius: 10px;
                        font-size: 16px;
                        font-family: 'Dosis', sans-serif;
                        color: var(--text-color-secondary);
                        cursor: pointer;
                        transition-duration: 0.3s;
                        user-select: none;

                        &:hover {
                            transition-duration: 0.3s;
                            background-color: var(--accent-color-secondary-theme);
                            color: var(--accent-color-secondary-theme-text);
                            outline: none;
                        }
                    }
                }
            }
            
            .courses{
                
                .coursesList {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(248px, 1fr));
                    gap: 24px;
                    align-items: start;

                    min-height: calc(80vh - 64px - 32px);
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