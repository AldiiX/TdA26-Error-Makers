import type { Course } from '#shared/types'
import getBaseUrl from '#shared/utils/getBaseUrl'
import { ref, watch } from 'vue'

const ALL_FIRST_FETCH_LIMIT = 24
const MY_FIRST_FETCH_LIMIT = 24

type CoursesResourceOptions = {
    stateKey: string
    hasFetchedKey: string
    fullFetchRunningKey: string
    firstFetchLimit: number
    listEndpoint: string
    fullEndpoint: string
    lazyKey: string
}

function createCoursesResource(options: CoursesResourceOptions) {
    const courses = useState<Course[] | null>(options.stateKey, () => null)
    const hasFetchedAll = useState<boolean>(options.hasFetchedKey, () => false)
    const fullFetchRunning = useState<boolean>(options.fullFetchRunningKey, () => false)

    async function fetchAllIfNeeded() {
        if (hasFetchedAll.value) return
        if (fullFetchRunning.value) return
        if (!courses.value) return

        // kdyz prvni fetch vratil mene nez limit, dalsi data uz pravdepodobne nejsou
        if (courses.value.length < options.firstFetchLimit) {
            hasFetchedAll.value = true
            return
        }

        fullFetchRunning.value = true
        try {
            courses.value = await $fetch<Course[]>(options.fullEndpoint, {
                baseURL: getBaseUrl(),
            })
            hasFetchedAll.value = true
        } catch (e) {
            console.error(`error fetching full list for ${options.stateKey}:`, e)
        } finally {
            fullFetchRunning.value = false
        }
    }

    const { pending, error } = useLazyFetch<Course[]>(
        `${options.listEndpoint}?limit=${options.firstFetchLimit}`,
        {
            baseURL: getBaseUrl(),
            server: false,
            key: options.lazyKey,
            getCachedData: () => courses.value ?? undefined,
            onResponse({ response }) {
                courses.value = response._data ?? null
                void fetchAllIfNeeded()
            },
        }
    )

    // kdyz data prisla z cache, onresponse se nespusti, tak tohle zajisti full fetch i v tom pripade
    watch(() => courses.value?.length, () => {
        void fetchAllIfNeeded()
    }, { immediate: true })

    async function refresh() {
        try {
            courses.value = await $fetch<Course[]>(options.fullEndpoint, {
                baseURL: getBaseUrl(),
            })
            hasFetchedAll.value = true
        } catch (e) {
            console.error(`error refreshing ${options.stateKey}:`, e)
        }
    }

    function invalidate() {
        // vymaze cached usestate pro dane klice
        clearNuxtState([options.stateKey, options.hasFetchedKey, options.fullFetchRunningKey])

        courses.value = null
        hasFetchedAll.value = false
        fullFetchRunning.value = false
    }

    return {
        courses,
        pending,
        error,
        refresh,
        invalidate,
    }
}

export function useCourses() {
    const all = createCoursesResource({
        stateKey: 'allCourses',
        hasFetchedKey: 'hasFetchedAllCourses',
        fullFetchRunningKey: 'allCoursesFullFetchRunning',
        firstFetchLimit: ALL_FIRST_FETCH_LIMIT,
        listEndpoint: '/api/v1/courses',
        fullEndpoint: '/api/v1/courses',
        lazyKey: `allCourses:limit:${ALL_FIRST_FETCH_LIMIT}`,
    })

    const my = createCoursesResource({
        stateKey: 'myCoursesCache',
        hasFetchedKey: 'hasFetchedAllMyCourses',
        fullFetchRunningKey: 'myCoursesFullFetchRunning',
        firstFetchLimit: MY_FIRST_FETCH_LIMIT,
        listEndpoint: '/api/v1/me/courses',
        fullEndpoint: '/api/v1/me/courses',
        lazyKey: `myCourses:limit:${MY_FIRST_FETCH_LIMIT}`,
    })

    function invalidateCoursesState() {
        all.invalidate()
        my.invalidate()
    }

    return {
        // public courses
        allCourses: all.courses,
        allCoursesPending: all.pending,
        allCoursesError: all.error,
        refreshAllCourses: all.refresh,
        invalidateAllCourses: all.invalidate,

        // my courses
        myCourses: my.courses,
        myCoursesPending: my.pending,
        myCoursesError: my.error,
        refreshMyCourses: my.refresh,
        invalidateMyCourses: my.invalidate,

        // convenience
        invalidateCoursesState,
    }
}