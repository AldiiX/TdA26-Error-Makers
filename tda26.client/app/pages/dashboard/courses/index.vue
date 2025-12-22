<script setup lang="ts">
import { Head, Title } from '#components';
import CourseCard from "~/components/pagespecific/CourseCard.vue";
import type {Account, Course} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import {computed, watch} from "vue";
import Modal from "~/components/Modal.vue";
import Button from "~/components/Button.vue";
import CourseForm from "~/components/pagespecific/CourseForm.vue";
import Pagination from "~/components/Pagination.vue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>('loggedAccount');
        if (!user.value) return navigateTo('/login');
    }
});



// Cache for all courses using useState
const allCoursesCache = useState<Course[] | null>('allCoursesCache', () => null);

// Non-blocking lazy fetch for all courses
const { data: _courses, pending, error, refresh } = useFetch<Course[]>(getBaseUrl() + '/api/v2/me/courses', {
    server: false,
    lazy: true
});

// Update cache when data is fetched
watch(_courses, (newCourses) => {
    if (newCourses) {
        allCoursesCache.value = newCourses;
    }
});

const loggedAccount = useState<Account>('loggedAccount');
const courses = computed(() => {
    // Prefer fresh data from fetch, fallback to cache if fetch hasn't completed yet
    return _courses.value ?? allCoursesCache.value ?? [];
});

const sort = ref<'new' | 'old'>('new');
const sortedCourses = computed(() => {
    let list = [...courses.value];

    switch (sort.value) {
        case 'new':
            return list.sort(
                (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
            );
        case 'old':
            return list.sort(
                (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
            );
        default:
            return list;
    }
});

const page = ref(1);
const PAGE_SIZE = 10;

const paginatedCourses = computed(() => {
    if (!courses.value) return [];
    const list = sortedCourses.value;
    const start = (page.value - 1) * PAGE_SIZE;
    return list.slice(start, start + PAGE_SIZE);
});

const totalPages = computed(() => {
    if (!courses.value) return 0;
    return Math.ceil(sortedCourses.value.length / PAGE_SIZE);
});

const goToPage = (newPage: number) => {
    if (newPage < 1 || newPage > totalPages.value) return;

    scrollTo({ top: window.innerHeight * 0.2, behavior: 'smooth' });
    page.value = newPage;
};

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

const enabledModal = ref<"createCourse" | "updateCourse" | "deleteCourse" | null>(null);
const editingCourseId = ref<string | null>(null);

const refreshCourses = async () => {
    try {
        const refreshed = await $fetch<Course[]>(getBaseUrl() + "/api/v2/me/courses");
        _courses.value = refreshed;
        allCoursesCache.value = refreshed;
    } catch {}
};

const openEdit = (course: Course) => {
    editingCourseId.value = course.uuid;
    enabledModal.value = "updateCourse";
};

const selectedDeleteCourse = ref<Course | null>(null);
const deleteError = ref<string | null>(null);

const openDelete = (course: Course) => {
    selectedDeleteCourse.value = course;
    enabledModal.value = "deleteCourse";
};

const deleteCourse = async () => {
    if (!selectedDeleteCourse.value) return;

    try {
        await $fetch(getBaseUrl() + `/api/v2/courses/${selectedDeleteCourse.value.uuid}`, {
            method: "DELETE"
        });

        enabledModal.value = null;
        selectedDeleteCourse.value = null;
        await refreshCourses();
    } catch (err) {
        deleteError.value = "Nepodařilo se smazat kurz.";
    }
};

</script>

<template>
    <Head>
        <Title>Moje kurzy • Think different Academy</Title>
    </Head>

    <h1 :class="$style.nadpis">
        Moje kurzy
        <span v-if="loggedAccount.type === 'Admin'" :class="$style.admininfo">(jste Admin, můžete spravovat všechny kurzy)</span>
    </h1>
<!--    <p :class="$style.podnapis">Lorem ipsum dolor sit amet, consectetur adipisicing elit...</p>-->

<!--  TODO: Místo kopírování kódu, tak udělat komponentu pro kurzy na courses a dashboard/courses  -->

    <div :class="$style.courses">
        <div :class="$style.coursesList">
            <CourseCard
                v-for="course in paginatedCourses"
                edit-mode
                :course="course"
                :key="course.uuid"
                @edit="openEdit(course)"
                @delete="openDelete(course)"
            />
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

    <Teleport to="#teleports">
        <!-- CREATE -->
        <Modal
            :enabled="enabledModal === 'createCourse'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
        >
            <h3>Vytvořit nový kurz</h3>
            <CourseForm
                mode="create"
                @finished="() => { enabledModal = null; refreshCourses(); }"
            />
        </Modal>

        <!-- EDIT -->
        <Modal
            :enabled="enabledModal === 'updateCourse'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
        >
            <h3>Upravit kurz</h3>
            <CourseForm
                mode="edit"
                :course-id="editingCourseId"
                @finished="() => { enabledModal = null; refreshCourses(); editingCourseId = null; }"
            />
        </Modal>

        <!-- DELETE -->
        <Modal :enabled="enabledModal === 'deleteCourse'" @close="enabledModal = null" can-be-closed-by-clicking-outside>
            <h3>Opravdu si přeješ smazat kurz <i class="text-gradient">{{ selectedDeleteCourse?.name }}</i>?</h3>
            <p>Tuto akci nelze vrátit zpět.</p>
            <div style="display: flex; gap: 16px; margin-top: 24px;">
                <Button button-style="tertiary" @click="enabledModal = null">Zrušit</Button>
                <Button
                    button-style="primary"
                    accent-color="secondary"
                    @click="deleteCourse"
                >
                    Smazat kurz
                </Button>
            </div>
            <p v-if="deleteError" class="error-text">{{ deleteError }}</p>
        </Modal>
    </Teleport>
</template>

<style module lang="scss">
.nadpis {
    font-size: 64px;
    margin: 0;

    .admininfo {
        font-size: 16px;
        color: var(--text-color);
        opacity: 0.5;
        margin-left: 16px;
        font-weight: 500;
    }
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
}


.courses{
    margin-top: 32px;
    
    .coursesList {
        display: grid;
        gap: 32px;
        grid-template-columns: repeat(auto-fill, minmax(348px, 1fr));
        align-items: stretch;
        width: 100%;

        min-height: auto;

        > * {
            width: 100%;
            height: auto;
            display: flex;
        }
    }
    
    .pagination {
        margin-top: 32px;
        display: flex;
    }
}
</style>