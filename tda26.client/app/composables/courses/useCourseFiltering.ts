import { computed, ref } from 'vue'
import type {Course, CourseStatus, Tag} from '#shared/types'



export function useCourseFiltering(
    courses: Ref<Course[] | null>,
    searchQuery: Ref<string>
) {

    const activeCategory = ref<string | null>(null)
    const activeTags = ref<string[]>([])
    const activeAuthor = ref<string | null>(null)
    const activeStatus = ref<CourseStatus | null>(null)
    const sort = ref<'new' | 'old' | 'byViews' | 'byLikes'>('new')

    const normalizeText = (str: string | null | undefined): string =>
        (str ?? '')
            .normalize('NFD')
            .replace(/\p{Diacritic}/gu, '')
            .replace(/\s+/g, ' ')
            .trim()
            .toLowerCase()

    // -----------------------------
    // Kategorie a tagy
    // -----------------------------
    const categoryIndex = computed(() => {
        const map = new Map<string, { tags: Map<string, Tag> }>()
        for (const course of courses.value ?? []) {
            const label = course.category?.label
            if (!label) continue

            if (!map.has(label)) map.set(label, { tags: new Map() })

            for (const tag of course.tags ?? []) {
                map.get(label)!.tags.set(tag.uuid, tag)
            }
        }
        return map
    })

    const categoryTags = computed(() => {
        if (!activeCategory.value) return []
        const tags = categoryIndex.value.get(activeCategory.value)?.tags
        if (!tags) return []
        return Array.from(tags.values()).sort((a, b) =>
            a.displayName.localeCompare(b.displayName)
        )
    })

    function toggleTag(uuid: string) {
        activeTags.value = activeTags.value.includes(uuid)
            ? activeTags.value.filter(t => t !== uuid)
            : [...activeTags.value, uuid]
    }

    function setCategory(category: string | null) {
        activeCategory.value = activeCategory.value === category ? null : category
        activeTags.value = []
    }

    // -----------------------------
    // Index pro hledání a řazení
    // -----------------------------
    const indexedCourses = computed(() => {
        return (courses.value ?? []).map(course => ({
            course,
            createdAtMs: Date.parse(course.createdAt) || 0,
            categoryLabel: course.category?.label ?? null,
            tagUuids: course.tags?.map(t => t.uuid) ?? [],
            searchText: normalizeText(
                `${course.name} ${course.description ?? ''} ${
                    course.tags?.map(t => t.displayName).join(' ') ?? ''
                }`
            ),
            status: course.status
        }))
    })

    // -----------------------------
    // Filtrování a řazení
    // -----------------------------
    const filteredCourses = computed(() => {
        let list = indexedCourses.value

        // Kategorie
        if (activeCategory.value) {
            list = list.filter(c => c.categoryLabel === activeCategory.value)
        }

        // Tagy
        if (activeTags.value.length) {
            list = list.filter(c =>
                c.tagUuids.some(t => activeTags.value.includes(t))
            )
        }

        // Autor
        if (activeAuthor.value) {
            list = list.filter(
                c => (c.course.lecturer?.uuid ?? c.course.account?.uuid) === activeAuthor.value
            )
        }

        // Stav
        if (activeStatus.value !== null) {
            list = list.filter(c => c.course.status === activeStatus.value)
        }

        // Vyhledávání
        if (searchQuery.value) {
            const q = normalizeText(searchQuery.value)
            list = list.filter(c => c.searchText.includes(q))
        }

        // Řazení
        list = list.slice()
        switch (sort.value) {
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

        return list.map(c => c.course)
    })

    // -----------------------------
    // Indikátor aktivních filtrů
    // -----------------------------
    const isAnyFilterActive = computed(() => {
        return (
            activeCategory.value !== null ||
            activeTags.value.length > 0 ||
            activeAuthor.value !== null ||
            activeStatus.value !== null ||
            searchQuery.value.trim() !== ''
        )
    })

    function resetAllFilters() {
        activeCategory.value = null
        activeTags.value = []
        activeAuthor.value = null
        activeStatus.value = null
        searchQuery.value = ''
    }

    return {
        sort,
        activeCategory,
        activeTags,
        activeAuthor,
        activeStatus,
        filteredCourses,
        toggleTag,
        setCategory,
        categoryIndex,
        categoryTags,
        isAnyFilterActive,
        resetAllFilters
    }
}
