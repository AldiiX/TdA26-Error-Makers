<script setup lang="ts">
    import { Head, Title } from '#components'
    import CourseCard from '~/components/pagespecific/CourseCard.vue'
    import type { Course } from '#shared/types'
    import NumberExponential from '~/components/NumberExponential.vue'
    import getBaseUrl from '#shared/utils/getBaseUrl'
    import Blob from '~/components/Blob.vue'
    import Tag from '~/components/Tag.vue'
    import SmoothSizeWrapper from '~/components/SmoothSizeWrapper.vue'
    import Pagination from '~/components/Pagination.vue'
    import Select from '~/components/Select.vue'

    definePageMeta({
        layout: 'normal-page-layout'
    })

    type TagItem = { uuid: string; displayName: string }

    type IndexedCourse = {
        course: Course
        createdAtMs: number
        categoryLabel: string | null
        tagUuids: string[]
        searchText: string
    }

    const PAGE_SIZE = 8
    const FIRST_FETCH_LIMIT = 25





    // -----------------------------
    // fetchovani
    // ----------------------------

    // sdilena cache mezi navigacemi
    const courses = useState<Course[] | null>('allCourses', () => null)

    // ochrany proti opakovanemu full fetchi
    const fullFetchRunning = ref(false)
    const hasFetchedAllCourses = useState<boolean>('hasFetchedAllCourses', () => false)

    async function fetchAllCoursesIfNeeded() {
        //if (hasFetchedAllCourses.value) { return }
        if (fullFetchRunning.value) { return }
        if (!courses.value) { return }

        // kdyz prvni fetch vratil mene nez limit, dalsi data uz pravdepodobne nejsou
        if (courses.value.length < FIRST_FETCH_LIMIT) {
            hasFetchedAllCourses.value = true
            return
        }

        fullFetchRunning.value = true
        try {
            const allCourses = await $fetch<Course[]>('/api/v2/courses', {
                baseURL: getBaseUrl()
            })

            courses.value = allCourses
            hasFetchedAllCourses.value = true
        } catch (e) {
            // kdyz se full fetch nepovede, nechceme to zamknout navzdy
            console.error('error fetching all courses:', e)
        } finally {
            fullFetchRunning.value = false
        }
    }

    const { pending, error } = useLazyFetch<Course[]>(`/api/v2/courses?limit=${FIRST_FETCH_LIMIT}`, {
        baseURL: getBaseUrl(),
        key: `allCourses:limit:${FIRST_FETCH_LIMIT}`,
        server: false,
        immediate: true,
        getCachedData: () => courses.value ?? undefined,
        onResponse({ response }) {
            courses.value = (response as any)._data as Course[]
            void fetchAllCoursesIfNeeded()
        }
    })

    // kdyz data prisla z cache, onresponse se nespusti, tak tohle zajisti upgrade fetch i v tom pripade
    watch(() => courses.value?.length, () => {
        void fetchAllCoursesIfNeeded()
    }, { immediate: true })

    watch(
        error,
        (e) => {
            if (e) {
                // logneme jen pro debug
                console.error('error fetching courses:', e)
            }
        },
        { immediate: true }
    )





    // -----------------------------
    // query param sync (?search=...)
    // -----------------------------
    const route = useRoute()
    const router = useRouter()

    function getQueryString(value: unknown): string {
        if (Array.isArray(value)) {
            return typeof value[0] === 'string' ? value[0] : ''
        }
        return typeof value === 'string' ? value : ''
    }

    const routeSearchQuery = computed(() => getQueryString(route.query.search))

    // filtrovani a strankovani
    const page = ref(1)

    // init hodnoty z url (aby input sedel hned pri prvnim renderu)
    const searchQuery = ref(routeSearchQuery.value)

    // filtry
    const activeTags = ref<string[]>([])
    const activeCategory = ref<string | null>(null)
    const activeAuthor = ref<string | null>(null)

    const sort = ref<'new' | 'old' | 'byViews' | 'byLikes'>('new')

    function normalizeText(str: string): string {
        return (str ?? '')
            .normalize('NFD')
            .replace(/\p{Diacritic}/gu, '')
            .replace(/\s+/g, ' ')
            .trim()
            .toLowerCase()
    }

    // debounce pro vyhledavani
    const debouncedQuery = ref('')
    let queryTimer: ReturnType<typeof setTimeout> | null = null

    watch(
        searchQuery,
        (val) => {
            if (queryTimer) {
                clearTimeout(queryTimer)
            }
            queryTimer = setTimeout(() => {
                debouncedQuery.value = val
            }, 200)
        },
        { immediate: true }
    )

    // kdyz se zmeni url (napr. back/forward nebo prime otevreni s jinym query), prepis input
    const syncingFromRoute = ref(false)

    watch(
        routeSearchQuery,
        async (val) => {
            if (val === searchQuery.value) { return }

            // zabrani zpetne smycce (route -> input -> replace -> route)
            syncingFromRoute.value = true
            searchQuery.value = val
            page.value = 1

            await nextTick()
            syncingFromRoute.value = false
        },
        { immediate: true }
    )

    // po debounce syncneme url (bez pridani do historie)
    watch(
        debouncedQuery,
        (val) => {
            if (syncingFromRoute.value) { return }

            const q = val.trim()
            const current = routeSearchQuery.value.trim()

            if (q === current) { return }

            router.replace({
                query: {
                    ...route.query,
                    // kdyz je prazdny, odstranime parametr z url
                    search: q || undefined
                }
            })

            page.value = 1
        }
    )





    // -----------------------------
    // filtrovani, razeni, strankovani
    // -----------------------------

    const activeTagSet = computed(() => new Set(activeTags.value))

    // kategorie + tagy pro vybranou kategorii v jednom pruchodu daty
    const categoryIndex = computed(() => {
        const map = new Map<string, { tags: Map<string, TagItem> }>()
        const list = courses.value ?? []

        for (const course of list) {
            const label = course.category?.label
            if (!label) continue

            let entry = map.get(label)
            if (!entry) {
                entry = { tags: new Map<string, TagItem>() }
                map.set(label, entry)
            }

            for (const tag of course.tags ?? []) {
                entry.tags.set(tag.uuid, { uuid: tag.uuid, displayName: tag.displayName })
            }
        }

        return map
    })

    // fetchnuti categories z api neblokujici
    const allCategories = useState<any[] | null>('courseCategories', () => null);

    onMounted(async () => {
        if(allCategories.value !== null) return

        try {
            const categories = await $fetch<any[]>('/api/v2/course-categories', {
                baseURL: getBaseUrl()
            })

            allCategories.value = categories.map((c) => c.label).sort((a, b) => a.localeCompare(b))
        } catch (e) {
            console.error('error fetching course categories:', e)
        }
    })

    const categoryTags = computed(() => {
        if (!activeCategory.value) return []
        const tags = categoryIndex.value.get(activeCategory.value)?.tags
        if (!tags) return []

        return Array.from(tags.values()).sort((a, b) => a.displayName.localeCompare(b.displayName))
    })

    // seznam unikatnich autoru
    const authorOptions = computed(() => {
        const list = courses.value ?? []
        const authorsMap = new Map<string, string>()

        for (const course of list) {
            // pokus se ziskat autora z lecturer nebo account
            let authorId: string | null = null
            let authorName: string | null = null

            if (course.lecturer) {
                authorId = course.lecturer.uuid
                authorName = course.lecturer.fullName
            } else if (course.account) {
                authorId = course.account.uuid
                authorName = course.account.fullName
            }

            if (authorId && authorName) {
                authorsMap.set(authorId, authorName)
            }
        }

        return Array.from(authorsMap.entries())
            .map(([value, label]) => ({ value, label }))
            .sort((a, b) => a.label.localeCompare(b.label))
    })

    function toggleTag(uuid: string) {
        activeTags.value = activeTags.value.includes(uuid)
            ? activeTags.value.filter((t) => t !== uuid)
            : [...activeTags.value, uuid]

        page.value = 1
    }

    function setCategory(category: string | null) {
        activeCategory.value = activeCategory.value === category ? null : category
        activeTags.value = []
        page.value = 1
    }

    // predpocitany index pro hledani/sort
    const indexedCourses = computed<IndexedCourse[]>(() => {
        const list = courses.value ?? []

        return list.map((course) => {
            const createdAtMs = Number.isFinite(Date.parse(course.createdAt))
                ? Date.parse(course.createdAt)
                : 0

            const categoryLabel = course.category?.label ?? null
            const tagUuids = (course.tags ?? []).map((t) => t.uuid)

            const searchText = normalizeText(
                [
                    course.name,
                    course.description ?? '',
                    (course.tags ?? []).map((t) => t.displayName).join(' ')
                ].join(' ')
            )

            return {
                course,
                createdAtMs,
                categoryLabel,
                tagUuids,
                searchText
            }
        })
    })

    const sortedCourses = computed<IndexedCourse[]>(() => {
        const list = indexedCourses.value.slice()

        switch (sort.value) {
            default:
            case 'new':
                list.sort((a, b) => b.createdAtMs - a.createdAtMs)
                break
            case 'old':
                list.sort((a, b) => a.createdAtMs - b.createdAtMs)
                break
            case 'byViews':
                list.sort((a, b) => (b.course.viewCount ?? 0) - (a.course.viewCount ?? 0))
                break
            case 'byLikes':
                list.sort((a, b) => (b.course.likeCount ?? 0) - (a.course.likeCount ?? 0))
                break
        }

        return list
    })

    const filteredCourses = computed<Course[]>(() => {
        let list = sortedCourses.value

        if (activeCategory.value) {
            list = list.filter((c) => c.categoryLabel === activeCategory.value)
        }

        if (activeTagSet.value.size > 0) {
            list = list.filter((c) => c.tagUuids.some((id) => activeTagSet.value.has(id)))
        }

        if (activeAuthor.value) {
            list = list.filter((c) => {
                const authorId = c.course.lecturer?.uuid ?? c.course.account?.uuid ?? null
                return authorId === activeAuthor.value
            })
        }

        const query = normalizeText(debouncedQuery.value)
        if (query) {
            list = list.filter((c) => c.searchText.includes(query))
        }

        return list.map((x) => x.course)
    })

    watch([sort, debouncedQuery, activeAuthor], () => {
        page.value = 1
    })

    const totalPages = computed(() => {
        const total = Math.ceil(filteredCourses.value.length / PAGE_SIZE)
        return total > 0 ? total : 1
    })

    const paginatedCourses = computed(() => {
        const start = (page.value - 1) * PAGE_SIZE
        return filteredCourses.value.slice(start, start + PAGE_SIZE)
    })

    function goToPage(newPage: number) {
        if (newPage < 1 || newPage > totalPages.value) return

        scrollTo({ top: window.innerHeight * 0.2, behavior: 'smooth' })
        page.value = newPage
    }

    const visiblePages = computed<(number | '...')[]>(() => {
        const total = totalPages.value
        const current = page.value

        if (total <= 8) {
            return Array.from({ length: total }, (_, i) => i + 1)
        }

        const result: (number | '...')[] = []
        const push = (v: number | '...') => {
            if (result[result.length - 1] === v) return
            result.push(v)
        }

        push(1)

        const start = Math.max(2, current - 2)
        const end = Math.min(total - 1, current + 2)

        if (start > 2) push('...')

        for (let i = start; i <= end; i++) {
            push(i)
        }

        if (end < total - 1) push('...')

        push(total)

        while (result.length > 8) {
            const firstDots = result.indexOf('...')
            if (firstDots !== -1) {
                result.splice(firstDots, 1)
            } else {
                result.splice(1, 1)
            }
        }

        return result
    })

    // resetovani filteru
    const isAnyFilterActive = computed<boolean>(() => {
        return (activeCategory.value !== null || activeTags.value.length > 0 || activeAuthor.value !== null || debouncedQuery.value.trim() !== '') || false
    })

    const filterButtonClicked = ref(false);

    function resetAllFilters() {
        activeCategory.value = null
        activeTags.value = []
        activeAuthor.value = null
        searchQuery.value = ''
        page.value = 1
        filterButtonClicked.value = true;
        setTimeout(() => {
            filterButtonClicked.value = false;
        }, 500);
    }
</script>

<template>
    <Head>
        <Title>Kurzy • Think different Academy</Title>
    </Head>

    <Teleport to="#teleports">
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
                <p :class="$style.podnapis">
                    Objev naši širokou nabídku kurzů, které pokrývají různé oblasti technologií a programování.
                    Ať už jsi začátečník nebo pokročilý, máme pro tebe kurz, který ti pomůže rozvíjet tvé dovednosti a znalosti.
                </p>
            </div>

            <div :class="$style.right">
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
                            <div :class="[$style.resetbutton, { [$style.fadeout]: filterButtonClicked } ]" v-show="isAnyFilterActive" @click="resetAllFilters" :title="'Resetovat filtry'"></div>
                        </transition>
                    </div>

                    <div :class="[$style.searchBar]">
                        <div :class="$style.searchIcon"></div>
                        <input
                                type="text"
                                placeholder="Hledat kurz..."
                                v-model="searchQuery"
                        />
                    </div>

                    <div :class="$style.cont" v-if="authorOptions.length > 0">
                        <p>Autor</p>
                        <Select
                                :options="authorOptions"
                                v-model="activeAuthor"
                                placeholder="Všichni autoři"
                                search-placeholder="Hledat autora..."
                        />
                    </div>

                    <SmoothSizeWrapper style="width: 100%">
                        <div :class="[$style.categories, $style.cont]" v-if="allCategories && allCategories?.length > 0">
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

                        <div :class="[$style.tags, $style.cont]" v-if="activeCategory !== null && categoryTags?.length > 0">
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
                        <div :class="$style.loading" v-if="courses === null"></div>

                        <div :class="$style.coursesList" v-else-if="paginatedCourses.length > 0">
                            <CourseCard
                                    v-for="(course, i) in paginatedCourses"
                                    :course="course"
                                    :key="course.uuid"
                                    :reveal-delay-ms="i * 200"
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

        .left {
            display: flex;
            flex-direction: column;
            min-width: 348px;
            width: 22%;
            border-radius: 24px;
            padding: 32px;
            overflow: hidden;

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
                        mask-image: url('../../public/icons/reset.svg');
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
                    margin-left: 12px;
                    font-weight: 600;
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
                        mask: url('../../public/icons/loading1.svg');
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
</style>