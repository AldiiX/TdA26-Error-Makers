<script setup lang="ts">
import { Head, Title } from '#components';
import type { Account, Course } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import CourseCard from "~/components/pagespecific/CourseCard.vue";
import { NuxtLink } from "#components";
import Button from "~/components/Button.vue";
import Modal from "~/components/Modal.vue";
import CourseForm from "~/components/pagespecific/CourseForm.vue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>('loggedAccount');
        if (!user.value) return navigateTo('/login');
    }
});

const user = useState<Account | null>('loggedAccount');

const enabledModal = ref<"createCourse" | "updateCourse" | "deleteCourse" | null>(null);
const editingCourseId = ref<string | null>(null);

const { data: _courses, pending: coursesPending } = await useFetch<Course[]>(getBaseUrl() + '/api/v2/me/courses?max=4', {
    server: false
});

const courses = computed<Course[]>(() => {
    return [...(_courses.value ?? [])];
});

const courseList = ref<HTMLElement | null>(null);

const scroll = (amount: number) => {
    const el = courseList.value;
    if (!el) return;
    el.scrollBy({ left: amount, behavior: "smooth" });
};

const canScrollLeft = ref(false);
const canScrollRight = ref(true);

const updateScroll = () => {
    const coursesEl = courseList.value;
    if (coursesEl) {
        canScrollLeft.value = coursesEl.scrollLeft > 0;
        canScrollRight.value = coursesEl.scrollLeft + coursesEl.clientWidth < coursesEl.scrollWidth;
    }
};

onMounted(() => {
    updateScroll();
    window.addEventListener('resize', updateScroll);
});

watch(courses, () => {
    nextTick(() => updateScroll());
    window.removeEventListener('resize', updateScroll);
});

const refreshCourses = async () => {
    try {
        const refreshed = await $fetch<Course[]>(getBaseUrl() + "/api/v2/me/courses?max=4");
        _courses.value = refreshed;
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
        <Title>Přehled • Think different Academy</Title>
    </Head>

    <h1 :class="$style.nadpis">Přehled</h1>
    <p :class="$style.podnapis">Lorem ipsum dolor sit amet, consectetur adipisicing elit...</p>

    <div :class="$style.actionButtons">
        <div @click="enabledModal = 'createCourse'" :class="$style.createCourse">
            <Button button-style="primary" accent-color="primary"><span :class="$style.icon"></span><p>Vytvořit nový kurz</p></Button>
        </div>
    </div>

    <div :class="[$style.courses]">
        <div :class="$style.header">
            <h2>Nedávné kurzy</h2>
            <NuxtLink to="/dashboard/courses" :class="['text-gradient']">Všechny kurzy</NuxtLink>
        </div>

        <p v-if="coursesPending">Načítání kurzů...</p>
        <p v-else-if="courses.length === 0">Žádné kurzy nenalezeny.</p>

        <div v-else>
            <button :class="['liquid-glass', $style.leftBtn, canScrollLeft && $style.active]" @click="scroll(-1000)"><span><</span></button>
            <button :class="['liquid-glass', $style.rightBtn, canScrollRight && $style.active]" @click="scroll(1000)"><span>></span></button>

            <ul ref="courseList" @scroll="updateScroll">
                <li v-for="course in courses" :key="course.uuid">
                    <CourseCard 
                        :course="course" 
                        edit-mode 
                        @edit="openEdit(course)"
                        @delete="openDelete(course)"
                    />
                </li>
            </ul>
        </div>
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
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
}

.actionButtons {
    margin-top: 32px;
    display: flex;
    gap: 16px;

    .createCourse {
        text-decoration: none;
        
        .icon {
            display: inline-block;
            width: 40px;
            height: 40px;
            mask-image: url('../../../public/icons/book.svg');
            mask-size: cover;
            background-color: white;
            margin-right: 8px;
            vertical-align: middle;
        }
        
        button {
            display: flex;
            align-items: center;
            padding: 8px 32px;
            margin-top: 4px;
            gap: 8px;
            
            p {
                font-size: 24px;
            }
        }
    }
}

.courses {
    position: relative;
    margin-top: 48px;
    margin-bottom: 16px;
    border-radius: 12px;
    overflow-x: hidden;
    
    a {
        text-decoration: none;
    }
    
    .header {
        display: flex;
        align-items: baseline;
        margin-bottom: 24px;
        gap: 8px;
        
        >h2 {
            margin: 0;
        }
        
        a {
            font-size: 20px;
            font-weight: 600;
            padding-bottom: 2px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
    }

    ul {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        gap: 32px;
        overflow-x: scroll;
        scroll-behavior: smooth;

        scrollbar-width: none;
        &::-webkit-scrollbar {
            display: none;
        }
        li {
            flex: 0 0 auto;
            overflow-y: hidden;

            display: block;
            box-shadow: var(--card-shadow);

            &:hover {
                box-shadow: none;
            }
            
            >* {
                box-shadow: none;
                //width: 400px;
                //height: 400px;
            }
        }
    }
}

.leftBtn,
.rightBtn {
    position: absolute;
    top: 50%;
    transform: translateY(-40%);
    border: none;
    background-color: var(--accent-color-primary-dark);
    color: white;
    font-size: 24px;
    width: 48px;
    height: 48px;
    border-radius: 100%;
    cursor: pointer;
    transition: all .2s;
    z-index: 2;
    margin: 0 16px;
    opacity: 0;
    pointer-events: none;
    display: flex;
    align-items: center;
    justify-content: center;

    &:hover {
        background-color: var(--accent-color-primary);
    }
    
    &.active {
        opacity: 1;
        pointer-events: auto;
    }
}

.leftBtn {
    left: 0;
}

.rightBtn {
    right: 0;
}
</style>
