
export function usePagination<T>(items: Ref<T[]>, pageSize = 8) {
    const page = ref(1)

    const totalPages = computed(() =>
        Math.max(1, Math.ceil(items.value.length / pageSize))
    )

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

    function goToPage(newPage: number) {
        if (newPage < 1 || newPage > totalPages.value) return

        scrollTo({ top: window.innerHeight * 0.2, behavior: 'smooth' })
        page.value = newPage
    }
    
    const paginatedItems = computed(() => {
        const start = (page.value - 1) * pageSize
        return items.value.slice(start, start + pageSize)
    })

    watch(items, () => page.value = 1)

    return {
        page,
        totalPages,
        visiblePages,
        goToPage,
        paginatedItems
    }
}