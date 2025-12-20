<script setup lang="ts">
import { Head, Title } from '#components';
import CourseCard from "~/components/pagespecific/CourseCard.vue";
import type {Account, Course} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import {computed} from "vue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>('loggedAccount');
        if (!user.value) return navigateTo('/login');
    }
});



const { data: _courses, pending, error, refresh } = await useFetch<Course[]>(getBaseUrl() + '/api/v2/me/courses', {
    server: false
});

const loggedAccount = useState<Account>('loggedAccount');
const courses = computed(() => _courses.value ?? []);

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
const PAGE_SIZE = 8;

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

const goToNextPage = () => {
    goToPage(page.value + 1);
};

const goToLastPage = () => {
    goToPage(page.value - 1);
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
                :course="course"
                :key="course.uuid"
            />
        </div>
        <div :class="$style.pagination">
            <!-- Pagination controls will go here -->
            <p @click="goToLastPage()">zpet</p>
            <p>Stránka {{ page }} z {{ totalPages }}</p>
            <p @click="goToNextPage()">dopredu</p>
        </div>
    </div>
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
        height: 64px;
        background-color: var(--accent-color-secondary-darker);
    }
}
</style>