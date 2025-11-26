<script setup lang="ts">
import { Head, Title } from '#components';
import type {Account, Course, CourseFormModel, CourseWithMaterials} from "#shared/types";
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

const enabledModal = ref<"createCourse" | "updateCourse" | null>(null);
const editingCourseId = ref<string | null>(null);

const { data: _courses, pending: coursesPending } = await useFetch<Course[]>(getBaseUrl() + '/api/v2/me/courses?max=4', {
    server: false
});

const courses = computed<CourseWithMaterials[]>(() => {
    const list = _courses.value ?? [];
    if (list.length === 0) return [];
    return [...list];
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
}

onMounted(() => {
    updateScroll();
    window.addEventListener('resize', updateScroll);
});

watch(courses, () => {
    nextTick(() => updateScroll());
    window.removeEventListener('resize', updateScroll);
});

const courseForm = ref<CourseFormModel>({
    name: '',
    description: '',
    materials: []
});

const createCourseLoading = ref(false);
const createCourseServerError = ref<string | null>(null);

const submitCourse = async (formData: any) => {
    createCourseLoading.value = true;
    createCourseServerError.value = null;

    try {
        const fd = new FormData();

        fd.append("Course.Name", formData.name);
        fd.append("Course.Description", formData.description ?? "");

        // FILE MATERIALS
        formData.materials
            .filter((m: any) => m.type === "file")
            .forEach((m: any, index: number) => {
                fd.append(`FileMaterials[${index}].Name`, m.name);
                fd.append(`FileMaterials[${index}].Type`, "file");
                if (m.file) fd.append(`FileMaterials[${index}].File`, m.file);
            });

        // URL MATERIALS
        formData.materials
            .filter((m: any) => m.type === "url")
            .forEach((m: any, index: number) => {
                fd.append(`UrlMaterials[${index}].Name`, m.name);
                fd.append(`UrlMaterials[${index}].Type`, "url");
                fd.append(`UrlMaterials[${index}].Url`, m.url ?? "");
            });
        
        // SEND AS MULTIPART/FORM-DATA
        await $fetch(getBaseUrl() + "/api/v2/courses", {
            method: "POST",
            body: fd,
        });

        // refresh courses display
        try {
            const refreshed = await $fetch<Course[]>(getBaseUrl() + "/api/v2/me/courses?max=4");
            _courses.value = refreshed;
        } catch {}

        // close modal + reset
        enabledModal.value = null;
        courseForm.value = { name: "", description: "", materials: [] };

    } catch (err: any) {
        createCourseServerError.value = err?.data?.message ?? err?.message ?? "Server error";
    } finally {
        createCourseLoading.value = false;
    }
};

const openEdit = async (course: CourseWithMaterials) => {
    const { data: _fullCourse } = await useFetch<CourseWithMaterials>(getBaseUrl() + `/api/v2/courses/${course.uuid}?full=true`, {
        server: false
    });
    
    const fullCourse = _fullCourse.value;
    if (!fullCourse) return;
    
    const safeMaterials = fullCourse.materials ?? [];

    courseForm.value = {
        name: fullCourse.name,
        description: fullCourse.description ?? "",
        materials: safeMaterials.map((m) => ({
            uuid: m.uuid,
            name: m.name,
            type: m.type,
            url: m.url ?? null,
            file: null
        }))
    };

    editingCourseId.value = fullCourse.uuid;
    enabledModal.value = "updateCourse";
};

const submitEditedCourse = async (formData: any) => {
    if (!editingCourseId.value) return;
    createCourseLoading.value = true;
    createCourseServerError.value = null;

    try {
        const fd = new FormData();

        fd.append("Course.Name", formData.name);
        fd.append("Course.Description", formData.description ?? "");
        fd.append("Course.Uuid", editingCourseId.value);

        // FILE MATERIALS
        formData.materials
            .filter((m: any) => m.type === "file")
            .forEach((m: any, index: number) => {
                fd.append(`FileMaterials[${index}].Uuid`, m.uuid);
                fd.append(`FileMaterials[${index}].Name`, m.name);
                fd.append(`FileMaterials[${index}].Type`, "file");
                if (m.file) fd.append(`FileMaterials[${index}].File`, m.file);
            });

        // URL MATERIALS
        formData.materials
            .filter((m: any) => m.type === "url")
            .forEach((m: any, index: number) => {
                fd.append(`UrlMaterials[${index}].Uuid`, m.uuid);
                fd.append(`UrlMaterials[${index}].Name`, m.name);
                fd.append(`UrlMaterials[${index}].Type`, "url");
                fd.append(`UrlMaterials[${index}].Url`, m.url ?? "");
            });

        await $fetch(getBaseUrl() + `/api/v2/courses/${editingCourseId.value}`, {
            method: "PUT",
            body: fd,
        });

        // refresh courses
        try {
            const refreshed = await $fetch<Course[]>(getBaseUrl() + "/api/v2/me/courses?max=4");
            _courses.value = refreshed;
        } catch {}

        enabledModal.value = null;
        courseForm.value = { name: "", description: "", materials: [] };
        editingCourseId.value = null;

    } catch (err: any) {
        createCourseServerError.value = err?.data?.message ?? err?.message ?? "Server error";
    } finally {
        createCourseLoading.value = false;
    }
};

</script>

<template>
    <Head>
        <Title>Dashboard • Think different Academy</Title>
    </Head>

    <h1 :class="$style.nadpis">Přehled</h1>
    <p :class="$style.podnapis">Lorem ipsum dolor sit amet, consectetur adipisicing elit. Adipisci at commodi corporis, delectus ducimus est facilis ipsum molestiae nisi odit, reprehenderit repudiandae sapiente sit temporibus voluptates. Consectetur cumque esse itaque.</p>

    <div :class="$style.actionButtons">
        <div @click="enabledModal = 'createCourse'" :class="$style.createCourse">
            <Button button-style="primary" accent-color="primary"><span :class="$style.icon"></span><p>Vytvořit nový kurz</p></Button>
        </div>
    </div>
    
    <div :class="['liquid-glass', $style.courses]">
        <div :class="$style.header">
            <h2>Moje kurzy</h2>
            <NuxtLink to="/dashboard/courses" :class="['text-gradient']">Všechny kurzy</NuxtLink>
        </div>
        <p v-if="coursesPending">Načítání kurzů...</p>
        <p v-else-if="courses.length === 0">Žádné kurzy nenalezeny.</p>

        <div v-else>
            <button :class="['liquid-glass', $style.leftBtn, canScrollLeft && $style.active]" @click="scroll(-1000)"><span><</span></button>
            <button :class="['liquid-glass', $style.rightBtn, canScrollRight && $style.active]" @click="scroll(1000)"><span>></span></button>

            <ul ref="courseList" @scroll="updateScroll">
                <li v-for="course in courses" :key="course!.uuid">
                    <CourseCard :course="course" edit-mode @edit="openEdit(course)" />
                </li>
            </ul>
        </div>
    </div>

    <Teleport to="#teleports">
        <Modal :enabled="enabledModal === 'createCourse'" @close="enabledModal = null" can-be-closed-by-clicking-outside>
            <h3 class="text-lg font-semibold">Vytvořit nový kurz</h3>

            <CourseForm
                v-model="courseForm"
                mode="create"
                @submit="submitCourse"
                @cancel="enabledModal = null"
            />
        </Modal>

        <Modal :enabled="enabledModal === 'updateCourse'" @close="enabledModal = null" can-be-closed-by-clicking-outside>
            <h3 class="text-lg font-semibold">Upravit kurz</h3>

            <CourseForm
                v-model="courseForm"
                mode="edit"
                @submit="submitEditedCourse"
                @cancel="enabledModal = null"
            />
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
            mask-image: url('../../public/icons/book.svg');
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
    padding: 32px;
    margin-bottom: 16px;
    border-radius: 12px;
    overflow-x: hidden;
    
    a {
        text-decoration: none;
    }
    
    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 24px;
        
        >h2 {
            margin: 0;
        }
        
        a {
            font-size: 24px;
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
                width: 400px;
                height: 400px;
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
