export function useSearchQuery() {
    const route = useRoute()
    const router = useRouter()

    function getQueryString(value: unknown): string {
        if (Array.isArray(value)) {
            return typeof value[0] === 'string' ? value[0] : ''
        }
        return typeof value === 'string' ? value : ''
    }
    
    const searchQuery = ref(getQueryString(route.query.search))
    const debouncedQuery = ref('')

    let timer: ReturnType<typeof setTimeout> | null = null
    const syncingFromRoute = ref(false)

    watch(searchQuery, (val) => {
        if (timer) clearTimeout(timer)
        timer = setTimeout(() => {
            debouncedQuery.value = val
        }, 200)
    }, { immediate: true })

    watch(() => route.query.search, (val) => {
        const q = typeof val === 'string' ? val : ''
        if (q === searchQuery.value) return

        syncingFromRoute.value = true
        searchQuery.value = q
        nextTick(() => syncingFromRoute.value = false)
    }, { immediate: true })

    watch(debouncedQuery, (val) => {
        if (syncingFromRoute.value) return

        router.replace({
            query: {
                ...route.query,
                search: val || undefined
            }
        })
    })

    return {
        searchQuery,
        debouncedQuery
    }
}