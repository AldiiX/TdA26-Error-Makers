import type { Course } from '#shared/types'
import getBaseUrl from '#shared/utils/getBaseUrl'

const FIRST_FETCH_LIMIT = 25

export function useCourses() {
    const courses = useState<Course[] | null>('allCourses', () => null)
    const hasFetchedAllCourses = useState<boolean>('hasFetchedAllCourses', () => false)
    const fullFetchRunning = ref(false)

    async function fetchAllCoursesIfNeeded() {
        if (fullFetchRunning.value || !courses.value || hasFetchedAllCourses.value) return

        if (courses.value.length < FIRST_FETCH_LIMIT) {
            hasFetchedAllCourses.value = true
            return
        }

        fullFetchRunning.value = true
        try {
            courses.value = await $fetch('/api/v2/courses', {
                baseURL: getBaseUrl()
            })
            hasFetchedAllCourses.value = true
        } finally {
            fullFetchRunning.value = false
        }
    }

    const { pending, error } = useLazyFetch<Course[]>(
        `/api/v2/courses?limit=${FIRST_FETCH_LIMIT}`,
        {
            baseURL: getBaseUrl(),
            server: false,
            getCachedData: () => courses.value ?? undefined,
            onResponse({ response }) {
                courses.value = response._data ?? null
                fetchAllCoursesIfNeeded()
            }
        }
    )

    watch(() => courses.value?.length, fetchAllCoursesIfNeeded, { immediate: true })

    return {
        courses,
        pending,
        error
    }
}