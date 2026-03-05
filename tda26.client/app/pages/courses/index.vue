<script setup lang="ts">
    import CourseCard from '~/components/pagespecific/CourseCard.vue'
    import type { Course } from '#shared/types'
    import NumberExponential from '~/components/NumberExponential.vue'
    import getBaseUrl from '#shared/utils/getBaseUrl'
    import Blob from '~/components/Blob.vue'
    import Tag from '~/components/Tag.vue'
    import SmoothSizeWrapper from '~/components/SmoothSizeWrapper.vue'
    import Pagination from '~/components/Pagination.vue'
    import Select from '~/components/Select.vue'
    import SelectLecturer from "~/components/pagespecific/SelectLecturer.vue";
    import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
    import { statusToText } from "#shared/utils/statusMapper";

    import { useCourses } from '~/composables/useCourses'
    import { useSearchQuery } from '~/composables/courses/useSearchQuery'
    import { useCourseFiltering } from '~/composables/courses/useCourseFiltering'
    import { usePagination } from '~/composables/courses/usePagination'
    import useAuth from "~/composables/useAuth";

    definePageMeta({
        layout: 'normal-page-layout'
    })

    // SEO
    useSeo({
        title: "Kurzy",
        description: "Procházejte širokou nabídku interaktivních online kurzů. Najděte kurzy z různých oborů a začněte se vzdělávat zábavnou formou.",
        keywords: "online kurzy, katalog kurzů, vzdělávací programy, e-learning kurzy, výuka online"
    });

    const { allCourses: courses } = useCourses();
    const { searchQuery, debouncedQuery } = useSearchQuery();
    const { loggedAccount } = useAuth();

    const {
        filteredCourses,
        activeCategory,
        activeTags,
        activeAuthor,
        activeStatus,
        setCategory,
        toggleTag,
        categoryTags,
        sort,
        resetAllFilters
    } = useCourseFiltering(courses, debouncedQuery)

    const {
        page,
        totalPages,
        visiblePages,
        goToPage,
        paginatedItems: paginatedCourses
    } = usePagination(filteredCourses)

    
    // // fetchnuti categories z api neblokujici
    const allCategories = useState<any[] | null>('courseCategories', () => null);

    onMounted(async () => {
        if(allCategories.value !== null) return

        try {
            const categories = await $fetch<any[]>('/api/v1/course-categories', {
                baseURL: getBaseUrl()
            })

            allCategories.value = categories.map((c) => c.label).sort((a, b) => a.localeCompare(b))
        } catch (e) {
            console.error('error fetching course categories:', e)
        }
    })
    
    // // seznam unikatnich autoru
    const authorOptions = computed(() => {
        const list = courses.value ?? []
        const arr: any[] = []

        for (const course of list) {
            const obj = {
                label: course.lecturer?.fullNameWithoutTitles
                    ?? course.account?.fullNameWithoutTitles
                    ?? 'Neznámý autor',
                value: course.lecturer?.uuid
                    ?? course.account?.uuid
                    ?? null,
                pictureUrl: course.lecturer?.pictureUrl
                    ?? null
            }

            if (!arr.find((a) => a.value === obj.value)) {
                arr.push(obj)
            }
        }

        return arr.sort((a, b) => a.label.localeCompare(b.label))
    })
    
    const stateOptions = [
        { value: 'draft', label: statusToText('draft') },
        { value: 'scheduled', label: statusToText("scheduled") },
        { value: 'live', label: statusToText('live') },
        { value: 'paused', label: statusToText('paused') },
        { value: 'archived', label: statusToText('archived') }
    ]
    const isAnyFilterActive = computed<boolean>(() => {
        return (activeCategory.value !== null || activeTags.value.length > 0 || activeAuthor.value !== null || debouncedQuery.value.trim() !== '') || activeStatus.value !== null || false
    })
    const filterButtonClicked = ref(false);

    function courseIsOpenable(course: Course): boolean {
        if(course.account?.uuid === loggedAccount.value?.uuid) return true;
        if(loggedAccount.value?.type === "admin") return true;

        return course.status === "live";
    }
</script>

<template>
    <Head>
        <Title>Kurzy • Think different Academy</Title>
    </Head>

    <Teleport to="#teleports">
<!--        <Blob-->
<!--                top="120vh"-->
<!--                right="1vw"-->
<!--                left="unset"-->
<!--                background="linear-gradient(0deg, var(&#45;&#45;accent-color-primary) 0%, transparent 80%)"-->
<!--                style="opacity: 0.5"-->
<!--                :class="$style.blob1"-->
<!--        />-->
<!--        <Blob-->
<!--                top="100vh"-->
<!--                left="2vw"-->
<!--                right="unset"-->
<!--                background="linear-gradient(0deg, transparent 0%, var(&#45;&#45;accent-color-secondary-theme)  80%)"-->
<!--                style="opacity: 0.5"-->
<!--                :class="$style.blob2"-->
<!--        />-->

        <CircleBlurBlob top="12.5vh" right="-12.5vw" color="var(--accent-color-primary)" size="20vw" blur="14vw" />
        <CircleBlurBlob top="100vh" left="-12.5vw" color="var(--accent-color-secondary-theme)" size="20vw" blur="14vw" />
    </Teleport>

    <section :class="$style.section">
        <div :class="$style.topContainer">
            <div :class="$style.left">
                <h1 :class="$style.nadpis">Kurzy</h1>
                <p :class="$style.podnapis">
                    Objev naši širokou nabídku kurzů, které pokrývají různé oblasti technologií a programování.
                    Ať už jsi začátečník nebo pokročilý, máme pro tebe kurz, který ti pomůže rozvíjet tvé dovednosti a znalosti.
                </p>
            </div>

            <div v-if="false" :class="$style.right">
                <div :class="$style.coursesInfo">
                    <div :class="$style.row">
                        <NumberExponential
                                :value="courses?.length ?? 0"
                                :decimals="0"
                                :duration-ms="1500"
                                :locale="'cs-CZ'"
                                :format-options="{ useGrouping: true }"
                                :number-class="$style.number"
                        />
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
            <div :class="[$style.left]">
                <div :class="$style.filtersLeft">
                    <div :class="$style.nadpis">
                        <p>Filtry</p>

                        <transition name="fade">
                            <div v-show="isAnyFilterActive" :class="[$style.resetbutton, { [$style.fadeout]: filterButtonClicked } ]" :title="'Resetovat filtry'" @click="resetAllFilters"/>
                        </transition>
                    </div>

                    <div :class="[$style.searchBar]">
                        <div :class="$style.searchIcon"/>
                        <input
                                v-model="searchQuery"
                                type="text"
                                placeholder="Hledat kurz..."
                        >
                    </div>

                    <div :class="[$style.cont, $style.author]">
                        <p>Autor</p>
                        <SelectLecturer
                                v-model="activeAuthor"
                                :options="authorOptions"
                                placeholder="Všichni autoři"
                                search-placeholder="Hledat autora..."
                                :dropdown-class="$style.sdd"
                                special-render="withAvatar"
                                :class="$style.selection"
                        />
                    </div>
                    
                    <div :class="[$style.state, $style.cont]">
                        <p>Stav</p>
                        <Select
                                v-model="activeStatus"
                                :options="stateOptions"
                                placeholder="Všechny stavy"
                                search-placeholder="Hledat stav..."
                                :class="$style.selection"
                                :dropdown-class="$style.sdd"
                        />
                    </div>

                    <SmoothSizeWrapper style="width: 100%; overflow: hidden;">
                        <div v-if="allCategories && allCategories?.length > 0" :class="[$style.categories, $style.cont]">
                            <p>Kategorie</p>

                            <div :class="$style.list">
                                <p
                                        v-for="category in allCategories"
                                        :key="category"
                                        :class="$style.tag"
                                        :data-active="activeCategory === category"
                                        @click="setCategory(category)"
                                >
                                    {{ category }}
                                </p>
                            </div>
                        </div>

                        <div v-if="activeCategory !== null && categoryTags?.length > 0" :class="[$style.tags, $style.cont]">
                            <p>Tagy</p>

                            <div :class="$style.sortOptions">
                                <Tag
                                        v-for="tag in categoryTags"
                                        :key="tag.uuid"
                                        :tag="tag"
                                        :active="activeTags.includes(tag.uuid)"
                                        @toggle="toggleTag"
                                />
                            </div>
                        </div>
                    </SmoothSizeWrapper>
                </div>
            </div>

            <div :class="$style.right">
                <div :class="$style.filtersTop">
                    <p>Seřadit: </p>
                    <div :class="$style.sortOptionsList">
                        <p
                                :class="$style.sortOption"
                                :data-active="sort === 'new'"
                                @click="sort = 'new'"
                        >Nejnovější</p>
                        <p
                                :class="$style.sortOption"
                                :data-active="sort === 'old'"
                                @click="sort = 'old'"
                        >Nejstarší</p>
                        <p
                                :class="$style.sortOption"
                                :data-active="sort === 'byLikes'"
                                @click="sort = 'byLikes'"
                        >Nejlepe hodnocení</p>
                        <p
                                :class="$style.sortOption"
                                :data-active="sort === 'byViews'"
                                @click="sort = 'byViews'"
                        >Nejvíce zhlédnutí</p>
                    </div>
                </div>

                <div :class="$style.courses">
                    <div :class="$style.coursesWrapper">
                        <div v-if="courses === null" :class="$style.loading"/>

                        <div v-else-if="paginatedCourses.length > 0" :class="$style.coursesList">
                            <CourseCard
                                    v-for="(course, i) in paginatedCourses"
                                    :key="course.uuid"
                                    :course="course"
                                    :reveal-delay-ms="i * 200"
                                    :openable="courseIsOpenable(course)"
                            />
                        </div>

                        <p v-else style="padding: 16px; color: var(--text-color-secondary);">
                            Nenašli jsme žádné kurzy podle zadaných filtrů.
                        </p>
                    </div>

                    <Pagination
                            v-if="totalPages > 1"
                            :page="page"
                            :total-pages="totalPages"
                            :visible-pages="visiblePages"
                            :class="$style.pagination"
                            @update:page="goToPage"
                    />
                </div>
            </div>
        </div>
    </section>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

/* zbytek stylu nechavam stejny */
.blob {
    width: 100vw;
    height: 100vh;
    position: absolute;
    left: 0;
    z-index: -1;
    animation: sdoksapkdf 1.5s forwards ease;
    top: -25vh;
    mask-image: linear-gradient(to bottom, #000 20%, transparent 85%);

    &::before {
        position: absolute;
        content: '';
        inset: 0;
        mask-image: url(../../../public/icons/blob_curses1.svg);
        mask-size: cover;
        mask-position: top;
        mask-repeat: no-repeat;
        background: linear-gradient(60deg, var(--accent-color-secondary-transparent-03), var(--accent-color));
        z-index: 0;
    }
}

.blob1 {
    animation: asdsasafasfasfhhdmg1 3s forwards ease;
}

.blob2 {
    animation: asdsasafasfasfhhdmg2 3s forwards ease;
}

.liquid-glass {
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 4px 24px rgba(0, 0, 0, 0.1);
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 1);
    backdrop-filter: blur(8px) saturate(1.6);
}

@keyframes asdsasafasfasfhhdmg1 {
    0% { opacity: 0; transform: translate(-40px, 80px); }
    20% { opacity: 0; transform: translate(-10px, 60px); }
    100% { opacity: 0.5; transform: translate(-50px, 0); }
}

@keyframes asdsasafasfasfhhdmg2 {
    0% { opacity: 0; transform: translate(-80px, 40px); }
    20% { opacity: 0; transform: translate(-60px, 10px); }
    100% { opacity: 0.5; transform: translate(0, -50px); }
}

@keyframes sdoksapkdf {
    from { transform: translateY(-80px); opacity: 0; }
    to { transform: translateY(0); opacity: 1; }
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
        align-items: start;
        z-index: 1;

        .left {
            display: flex;
            flex-direction: column;
            min-width: 348px;
            width: 22%;
            border-radius: 24px;
            padding: 32px;

            @extend .liquid-glass;

            .filtersLeft {
                >.nadpis {
                    position: relative;
                    display: flex;
                    margin-bottom: 16px;
                    align-items: center;
                    justify-content: space-between;

                    >p{
                        font-size: 36px;
                        font-weight: 700;
                        margin: 0;
                    }

                    .resetbutton {
                        width: 16px;
                        aspect-ratio: 1/1;
                        background-color: var(--text-color-primary);
                        mask-image: url(../../../public/icons/reset.svg);
                        mask-size: cover;
                        mask-position: center;
                        mask-repeat: no-repeat;
                        cursor: pointer;
                        transition-duration: 0.3s;

                        &:is(.fadeout) {
                            @keyframes rotateiojsdfodjij {
                                from { transform: rotate(0deg); opacity: 1; }
                                to { transform: rotate(360deg); opacity: 0; }
                            }

                            animation: rotateiojsdfodjij 0.4s forwards ease;
                            animation-iteration-count: 1;
                            opacity: 0;
                            pointer-events: none;
                        }

                        &:hover {
                            transition-duration: 0.3s;
                            opacity: 0.5;
                        }
                    }
                }


                .searchBar {
                    width: 100%;
                    background-color: var(--background-color-3);
                    display: flex;
                    align-items: center;
                    justify-self: center;
                    gap: 10px;
                    border-radius: 12px;
                    padding: 12px 16px;
                    transition: all 0.2s ease-in-out;
                    border: none;
                    font-family: Dosis, sans-serif;

                    .searchIcon {
                        mask-image: url('../../../public/icons/search.svg');
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

                        &::placeholder {
                            color: var(--text-color-secondary);
                            opacity: 0.8;
                            user-select: none;
                        }
                    }
                }

                .cont {
                    margin-top: 32px;

                    &:is(.categories) {
                        .list {
                            display: flex;
                            flex-wrap: wrap;
                            gap: 8px;

                            .tag {
                                background-color: var(--background-color-3);
                                padding: 8px 16px;
                                border-radius: 999px;
                                margin: 0;
                                font-size: 14px;
                                cursor: pointer;
                                user-select: none;
                                transition-duration: 0.3s;
                            }

                            .tag:hover {
                                background: var(--background-color-primary);
                            }

                            .tag[data-active="true"] {
                                background: var(--accent-color-primary);
                                color: var(--accent-color-primary-text);
                            }
                        }
                    }

                    &:is(.author) {
                        .sdd {
                            max-height: 200px;
                        }


                        .selection {
                            height: 48px;
                        }
                    }

                    >p {
                        font-size: 20px;
                        font-weight: 600;
                        margin: 0;
                        margin-bottom: 12px;
                    }

                    .sortOptions {
                        display: flex;
                        flex-wrap: wrap;
                        gap: 8px;
                    }
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
                gap: 16px 48px;
                padding: 20px;
                min-height: 64px;
                width: 100%;
                border-radius: 24px;
                flex-wrap: wrap;

                @extend .liquid-glass;

                >p {
                    font-size: 18px;
                    color: var(--text-color-secondary);
                    margin: 0;
                    margin-left: 4px;
                    font-weight: 600;
                }

                .sortOptionsList{
                    display: flex;
                    gap: 12px;
                    align-items: center;
                    flex-wrap: wrap;
                    margin: 0;

                    .sortOption {
                        appearance: none;
                        border: none;
                        background: transparent;
                        padding: 8px 12px;
                        border-radius: 10px;
                        font-size: 16px;
                        color: var(--text-color-secondary);
                        cursor: pointer;
                        transition-duration: 0.3s;
                        user-select: none;
                        margin: 0;

                        &:hover {
                            transition-duration: 0.3s;
                            background-color: var(--accent-color-secondary-theme);
                            color: var(--accent-color-secondary-theme-text);
                            outline: none;
                        }

                        &[data-active="true"] {
                            background-color: var(--accent-color-primary);
                            color: var(--accent-color-primary-text);
                        }
                    }
                }
            }

            .courses{
                .coursesWrapper {
                    min-height: 50vh;

                    .loading {
                        width: 64px;
                        height: 64px;
                        mask: url('../../../public/icons/loading1.svg');
                        mask-size: contain;
                        mask-repeat: no-repeat;
                        mask-position: center;
                        background-color: var(--accent-color-secondary-theme);
                    }

                    .coursesList {
                        display: grid;
                        gap: 32px;
                        grid-template-columns: repeat(auto-fill, minmax(348px, 1fr));
                        align-items: stretch;
                        width: 100%;
                        min-height: auto;

                        > * {
                            width: 100%;
                            min-height: auto;
                            height: auto;
                            display: flex;
                        }
                    }
                }

                .pagination {
                    margin-top: 32px;
                    display: flex;
                }
            }
        }
    }
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
    .bottomContainer {
        .right {
            .courses {
                .coursesWrapper {
                    .coursesList {
                        grid-template-columns: repeat(auto-fill, minmax(100%, 1fr)) !important;
                    }
                }
            }
        }
    }
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
    .section {
        margin-top: -50px;
    }
}

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
    .section {
        .topContainer {
            flex-direction: column;
            align-items: center;
            text-align: center;

            .right {
                width: auto;
                margin-top: 32px;
            }
        }

        .bottomContainer {
            flex-direction: column;
            gap: 80px;

            .left {
                width: 100%;
                min-width: auto;
                margin-top: 32px;
                overflow: visible;
            }

            .right {
                width: 100%;
                
                .course {
                    grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
                }
            }
        }
    }
}
</style>