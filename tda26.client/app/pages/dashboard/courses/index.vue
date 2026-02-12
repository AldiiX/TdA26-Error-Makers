<script setup lang="ts">
import {Head, Title} from '#components';
import CourseCard from "~/components/pagespecific/CourseCard.vue";
import type {Account, Course} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import {computed, watch} from "vue";
import Modal from "~/components/Modal.vue";
import ModalDestructive from "~/components/ModalDestructive.vue";
import Button from "~/components/Button.vue";
import CourseForm from "~/components/pagespecific/CourseForm.vue";
import Pagination from "~/components/Pagination.vue";
import { push } from "notivue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>('loggedAccount');
        if (!user.value) return navigateTo('/login');

        if(!(user.value.type === 'lecturer' || user.value.type === 'admin'))
            return navigateTo("/")
    }
});

const loggedAccount = useState<Account>('loggedAccount');

const PAGE_SIZE = 12;
const FIRST_FETCH_LIMIT = 24;

// sdilena cache mezi navigacemi
const coursesState = useState<Course[] | null>('myCoursesCache', () => null);

// ochrany proti opakovanemu full fetchi
const fullFetchRunning = ref(false);
const hasFetchedAllCourses = useState<boolean>('hasFetchedAllMyCourses', () => false);

async function fetchAllCoursesIfNeeded() {
    if (hasFetchedAllCourses.value) return;
    if (fullFetchRunning.value) return;
    if (!coursesState.value) return;

    // kdyz prvni fetch vratil mene nez limit, dalsi data uz pravdepodobne nejsou
    if (coursesState.value.length < FIRST_FETCH_LIMIT) {
        hasFetchedAllCourses.value = true;
        return;
    }

    fullFetchRunning.value = true;
    try {
        coursesState.value = await $fetch<Course[]>('/api/v2/me/courses', {
            baseURL: getBaseUrl()
        });

        hasFetchedAllCourses.value = true;
    } catch (e) {
        // kdyz se full fetch nepovede, nechceme to zamknout navzdy
        console.error('error fetching all my courses:', e);
    } finally {
        fullFetchRunning.value = false;
    }
}

// prvni rychly fetch xx kurzů
const { pending, error } = useLazyFetch<Course[]>(`/api/v2/me/courses?limit=${FIRST_FETCH_LIMIT}`, {
    baseURL: getBaseUrl(),
    key: `myCourses:limit:${FIRST_FETCH_LIMIT}`,
    server: false,
    immediate: true,
    getCachedData: () => coursesState.value ?? undefined,
    onResponse({ response }) {
        coursesState.value = (response as any)._data as Course[];
        void fetchAllCoursesIfNeeded();
    }
});

// kdyz data prisla z cache, onresponse se nespusti, tak tohle zajisti upgrade fetch i v tom pripade
watch(() => coursesState.value?.length, () => {
    void fetchAllCoursesIfNeeded();
}, { immediate: true });

watch(error, (e) => {
    if (e) {
        console.error('error fetching my courses:', e);
    }
}, { immediate: true });

const courses = computed(() => coursesState.value);

const sort = ref<'new' | 'old'>('new');
const sortedCourses = computed(() => {
    const list = courses.value ? [...courses.value] : [];

    switch (sort.value) {
        case 'new':
            return list.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        case 'old':
            return list.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
        default:
            return list;
    }
});

const page = ref(1);

const paginatedCourses = computed(() => {
    const list = sortedCourses.value;
    const start = (page.value - 1) * PAGE_SIZE;
    return list.slice(start, start + PAGE_SIZE);
});

const totalPages = computed(() => {
    return Math.ceil(sortedCourses.value.length / PAGE_SIZE);
});

const goToPage = (newPage: number) => {
    if (newPage < 1 || newPage > totalPages.value) return;

    scrollTo({ top: window.innerHeight * 0.2, behavior: 'smooth' });
    page.value = newPage;
};

const visiblePages = computed<(number | '...')[]>(() => {
    const total = totalPages.value;
    const current = page.value;

    if (total <= 8) {
        return Array.from({ length: total }, (_, i) => i + 1);
    }

    const result: (number | '...')[] = [];
    const push = (v: number | '...') => {
        if (result[result.length - 1] === v)  return;
        result.push(v);
    };

    push(1);

    const start = Math.max(2, current - 2);
    const end = Math.min(total - 1, current + 2);

    if (start > 2) push('...');

    for (let i = start; i <= end; i++) {
        push(i);
    }

    if (end < total - 1) push('...');

    push(total);

    while (result.length > 8) {
        const firstDots = result.indexOf('...');
        if (firstDots !== -1) {
            result.splice(firstDots, 1);
        } else {
            result.splice(1, 1);
        }
    }

    return result;
});

const enabledModal = ref<"createCourse" | "updateCourse" | "deleteCourse" | null>(null);
const editingCourseId = ref<string | null>(null);

const refreshCourses = async () => {
    try {
        coursesState.value = await $fetch<Course[]>('/api/v2/me/courses', {
            baseURL: getBaseUrl()
        });

        hasFetchedAllCourses.value = true;
    } catch {}
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

        push.success({
            title: "Kurz smazán",
            message: "Kurz byl úspěšně smazán.",
            duration: 4000
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
    
    <section>
        <h1 :class="$style.nadpis">
            Moje kurzy
            <span v-if="loggedAccount.type === 'admin'" :class="$style.admininfo">(jsi Admin, můžeš spravovat úplně všechny kurzy)</span>
        </h1>
        <!--    <p :class="$style.podnapis">Lorem ipsum dolor sit amet, consectetur adipisicing elit...</p>-->

        <!--  TODO: Místo kopírování kódu, tak udělat komponentu pro kurzy na courses a dashboard/courses  -->

        <div :class="$style.courses">
            <template v-if="courses === null">
                <p>Načítání kurzů...</p>
            </template>

            <template v-else-if="courses.length === 0">
                <p>Ještě nemáš žádné kurzy. Začni vytvořením nového kurzu kliknutím na tlačítko výše.</p>
            </template>

            <template v-else-if="courses.length > 0">
                <div :class="$style.coursesList">
                    <CourseCard
                        v-for="course in paginatedCourses"
                        edit-mode
                        :course="course"
                        :key="course.uuid"
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
            </template>
        </div>
    </section>

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
        <ModalDestructive
            :enabled="enabledModal === 'deleteCourse'"
            @close="enabledModal = null"
            :can-be-closed-by-clicking-outside="true"
            :title="`Potvrzení akce`"
            :description="`Opravdu si přeješ smazat kurz ${selectedDeleteCourse?.name ?? ''}? Tuto akci nelze vrátit zpět.`"
            :yes-action="deleteCourse"
            :no-action="() => enabledModal = null"
            :yes-text="`Smazat kurz`"
            :no-text="`Zrušit`"
        />
    </Teleport>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.nadpis {
    font-size: 64px;
    margin: 0;
    display: flex;
    flex-direction: column;

    .admininfo {
        font-size: 16px;
        color: var(--text-color);
        opacity: 0.5;
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

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
    section {
        margin-top: -50px;
    }

    .coursesList {
        grid-template-columns: repeat(auto-fill, minmax(100%, 1fr)) !important;
    }
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
}
</style>