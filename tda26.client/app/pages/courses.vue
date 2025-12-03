<script setup lang="ts">
    import { Head, Title } from '#components';
    import CourseCard from '~/components/pagespecific/CourseCard.vue';
    import type {Course, Lecturer} from '#shared/types';
    import NumberExponential from "~/components/NumberExponential.vue";
    import getBaseUrl from "#shared/utils/getBaseUrl";
    import Blob from "~/components/Blob.vue";
    import Tag from "~/components/Tag.vue";
    
    definePageMeta({
        layout: "normal-page-layout"
    });

    // client-side fetch courses
    onMounted(async () => {
        try {
            const res = await fetch(getBaseUrl() + '/api/v2/courses', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            if (!res.ok) {
                throw new Error('Failed to fetch courses');
            }
            const data = await res.json();
            courses.value = data;
        } catch (error) {
            console.error('Error fetching courses:', error);
        }
    });

    const courses = ref<Course[] | null>(null);
    
    // Filtrovani a strankovani
    
    const page = ref(1);
    const PAGE_SIZE = 8;
    
    const searchQuery = ref("");
    
    // Filtrovani 
    const activeTags = ref<string[]>([]);
    
    const allTags = computed(() => {
        const map = new Map<string, { uuid: string; displayName: string }>();

        courses.value?.forEach(course => {
            course.tags?.forEach(tag => {
                map.set(tag.uuid, tag);
            });
        });

        return Array.from(map.values());
    });
    const toggleTag = (uuid: string) => {
        if (activeTags.value.includes(uuid)) {
            activeTags.value = activeTags.value.filter(t => t !== uuid);
        } else {
            activeTags.value.push(uuid);
        }

        page.value = 1;
    };

    const sort = ref<'new' | 'old' | 'byViews' | 'byLikes'>('new');
    const sortedCourses = computed(() => {
        if (!courses.value) return [];
        let list = [...courses.value];

        switch (sort.value) {
            default:
            case 'new':
                return list.sort(
                    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
                );
            case 'old':
                return list.sort(
                    (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
                );
            case 'byViews':
                return list.sort((a, b) => b.viewCount - a.viewCount);
            case 'byLikes':
                return list.sort((a, b) => b.likeCount - a.likeCount);
        }
    });
    const filteredCourses = computed(() => {
        let list = sortedCourses.value;

        // Tagy(Filtry)
        if (activeTags.value.length > 0) {
            list = list.filter(course =>
                course.tags?.some(tag => activeTags.value.includes(tag.uuid))
            );
        }

        // Vyhledávání

        const normalize = (str: string) => {
            return str
                .normalize("NFD")          // oddělí diakritiku
                .replace(/\p{Diacritic}/gu, "") // odstraní diakritiku
                .replace(/\s+/g, " ")      // více mezer → 1 mezera
                .trim()                    // ořízne mezery
                .toLowerCase();
        };
        
        const query = normalize(searchQuery.value);

        if (query !== "") {
            list = list.filter(course => {
                const name = normalize(course.name);
                const desc = normalize(course.description ?? "");
                const tags = course.tags?.map(t => normalize(t.displayName)) ?? [];

                return (
                    name.includes(query) ||
                    desc.includes(query) ||
                    tags.some(t => t.includes(query))
                );
            });
        }

        return list;
    });
    
    
    
    // Strankovani
    const paginatedCourses = computed(() => {
        const list = filteredCourses.value;
        const start = (page.value - 1) * PAGE_SIZE;
        return list.slice(start, start + PAGE_SIZE);
    });

    const totalPages = computed(() => {
        return Math.ceil(filteredCourses.value.length / PAGE_SIZE);
    });
    
    const goToPage = (newPage: number) => {
        if (newPage < 1 || newPage > totalPages.value) return;

        scrollTo({ top: window.innerHeight * 0.2, behavior: 'smooth' });
        page.value = newPage;
    };

    const goToNextPage = () => {
        if (page.value < totalPages.value) goToPage(page.value + 1);
    };

    const goToLastPage = () => {
        if (page.value > 1) goToPage(page.value - 1);
    };
    
    const goToInput = ref<number | null>(null);

    const visiblePages = computed<(number | "...")[]>(() => {
        const total = totalPages.value;
        const current = page.value;

        if (total <= 8) {
            return Array.from({ length: total }, (_, i) => i + 1);
        }

        const slots = new Array(8).fill("...") as (number | "...")[];

        slots[0] = 1;
        slots[7] = total;

        let c1 = current - 1;
        let c2 = current;
        let c3 = current + 1;
        let c4 = current + 2;

        if (current <= 5) {
            c1 = 2;
            c2 = 3;
            c3 = 4;
            c4 = 5;
            slots[6] = 6;
        }

        else if (current >= total - 2) {
            c4 = total - 1;
            c3 = total - 2;
            c2 = total - 3;
            c1 = total - 4;
        }

        if (current <= 5) {
            return [1, 2, 3, 4, 5, 6, "...", total];
        }
        
        c1 = Math.max(2, Math.min(total - 1, c1));
        c2 = Math.max(2, Math.min(total - 1, c2));
        c3 = Math.max(2, Math.min(total - 1, c3));
        c4 = Math.max(2, Math.min(total - 1, c4));

        slots[2] = c1;
        slots[3] = c2;
        slots[4] = c3;
        slots[5] = c4;

        if (c1 === 2) slots[1] = 2;
        else slots[1] = "...";

        if (c4 === total - 1) slots[6] = total - 1;
        else slots[6] = "...";

        const finalSlots: (number | "...")[] = [];
        const used = new Set<number>();

        for (const s of slots) {
            if (typeof s === "number") {
                if (used.has(s)) continue;
                used.add(s);
            }
            finalSlots.push(s);
        }

        return finalSlots;
    });
    
</script>

<template>
    <Head>
        <Title>Kurzy • Think different Academy</Title>
    </Head>
    
    <Teleport to="#teleports">
<!--        <div :class="$style.blob"></div>   TODO: udělat toto pěkným     -->
        <Blob
            top="120vh"
            right="1vw"
            left="unset"
            background="linear-gradient(0deg, var(--accent-color-primary) 0%, transparent 80%)"
            style="opacity: 0.5"
            :class="$style.blob1"
        />
        <Blob
            top="100vh"
            left="2vw"
            right="unset"
            background="linear-gradient(0deg, transparent 0%, var(--accent-color-secondary-theme)  80%)"
            style="opacity: 0.5"
            :class="$style.blob2"
        />
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
                        <input type="text"
                               placeholder="Hledat kurz..."
                               v-model="searchQuery" />
                    </div>
                    <div :class="$style.sortOptions">
                        <Tag
                            v-for="tag in allTags"
                            :key="tag.uuid"
                            :tag="tag"
                            :active="activeTags.includes(tag.uuid)"
                            @toggle="toggleTag"
                        />
                    </div>
                </div>
            </div>
            
            <div :class="$style.right">
                <div :class="$style.filtersTop">
                    <p>Seřadit: </p>
                    <div :class="$style.sortOptionsList">
                        <button type="button" 
                                :class="$style.sortOption" 
                                :data-active="sort === 'new'"
                                @click="sort = 'new'">Nejnovější
                        </button>
                        <button type="button" 
                                :class="$style.sortOption" 
                                :data-active="sort === 'old'"
                                @click="sort = 'old'">Nejstarší
                        </button>
                        <button type="button" 
                                :class="$style.sortOption"
                                :data-active="sort === 'byLikes'"
                                @click="sort = 'byLikes'">Nejlepe hodnocení
                        </button>
                        <button type="button" 
                                :class="$style.sortOption"
                                :data-active="sort === 'byViews'"
                                @click="sort = 'byViews'">Nejvíce zhlédnutí
                        </button>
                    </div>
                </div>

                <div :class="$style.courses">
                    <div :class="$style.coursesWrapper">
                        <div :class="$style.coursesList">
                            <CourseCard
                                v-for="(course, i) in paginatedCourses"
                                :course="course"
                                :key="course.uuid"
                                :reveal-delay-ms="i * 200"
                            />
                        </div>
                    </div>
                    <Pagination
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



.blob {
    width: 100vw;
    height: 100vh;
    position: absolute;
    //top: 0;
    left: 0;
    z-index: -1;
    animation: sdoksapkdf 1.5s forwards ease;
    top: -25vh;

    // tady blob jako celek postupne mizi do pruhledna
    mask-image: linear-gradient(to bottom, #000 20%, transparent 85%);

    &::before {
        position: absolute;
        content: '';
        inset: 0;
        mask-image: url("../../public/icons/blob_curses1.svg");
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
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 4px 30px rgba(0, 0, 0, 0.15);
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 1);
    backdrop-filter: blur(8px) saturate(1.6);
}

@keyframes asdsasafasfasfhhdmg1 {
    0% {
        opacity: 0;
        transform: translate(-40px, 80px);
    }

    20% {
        opacity: 0;
        transform: translate(-10px, 60px);
    }

    100% {
        opacity: 0.5;
        transform: translate(-50px, 0);
    }
}

@keyframes asdsasafasfasfhhdmg2 {
    0% {
        opacity: 0;
        transform: translate(-80px, 40px);
    }

    20% {
        opacity: 0;
        transform: translate(-60px, 10px);
    }

    100% {
        opacity: 0.5;
        transform: translate(0, -50px);
    }
}

@keyframes sdoksapkdf {
    from {
        transform: translateY(-80px);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
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
            border-radius: 24px;
            padding-top: 8px;

            @extend .liquid-glass;

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
                    border: none;

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

                        &::placeholder {
                            color: var(--text-color-secondary);
                            opacity: 0.8;
                        }
                    }
                }

                .sortOptions {
                    display: flex;
                    flex-wrap: wrap;
                    padding: 8px;
                    gap: 12px;
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
                height: 64px;
                width: 100%;
                border-radius: 24px;

                @extend .liquid-glass;
                
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

                        &[data-active="true"] {
                            background-color: var(--accent-color-primary);
                            color: var(--accent-color-primary-text);
                            font-weight: 600;
                        }
                    }
                }
            }
            
            .courses{
                .coursesWrapper {
                    min-height: 50vh;

                    .coursesList {
                        display: grid;
                        gap: 32px;
                        grid-template-columns: repeat(auto-fill, minmax(324px, 1fr));
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
</style>