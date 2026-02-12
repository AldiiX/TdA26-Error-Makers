<script setup lang="ts">
import type {Account, Course, CourseCategory} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import CourseCard from "~/components/pagespecific/CourseCard.vue";
import { NuxtLink } from "#components";
import Button from "~/components/Button.vue";
import Modal from "~/components/Modal.vue";
import CourseForm from "~/components/pagespecific/CourseForm.vue";
import {computed, ref} from "vue";
import ModalDestructive from "~/components/ModalDestructive.vue";
import { onMounted, onUnmounted, nextTick, watch } from "vue";
import { useSeo } from "~/composables/useSeo";
import { push } from "notivue"

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>('loggedAccount');
        if (!user.value) return navigateTo('/login');

        if(!(user.value.type === 'lecturer' || user.value.type === 'admin'))
            return navigateTo("/")
    }
});

// SEO
useSeo({
    title: "Dashboard",
    description: "Spravujte své kurzy, sledujte statistiky a přístup ke všem funkcím vaší vzdělávací platformy.",
    noindex: true // Dashboard should not be indexed
});

const loggedAccount = useState<Account | null>('loggedAccount');

const isActionInProgress = ref(false);

const enabledModal = ref<"createCourse" | "updateCourse" | "deleteCourse" | null>(null);
const editingCourseId = ref<string | null>(null);

// Cache for courses using useState
const coursesCache = useState<Course[] | null>('dashboardCoursesCache', () => null);

// Non-blocking lazy fetch
const { data: _courses, pending: coursesPending } = useFetch<Course[]>(getBaseUrl() + '/api/v2/me/courses?limit=6', {
    server: false,
    lazy: true
});

// Update cache when data is fetched
watch(_courses, (newCourses) => {
    if (newCourses) {
        coursesCache.value = newCourses;
    }
});

const courses = computed<Course[]>(() => {
    // Prefer fresh data from fetch, fallback to cache if fetch hasn't completed yet
    return [...(_courses.value ?? coursesCache.value ?? [])];
});

const courseList = ref<HTMLElement | null>(null);

const { data: categories } = await useFetch<CourseCategory[]>(
    getBaseUrl() + "/api/v2/course-categories",
    { server: false }
);

const scroll = (direction: -1 | 1) => {
    const el = courseList.value;
    if (!el) return;
    el.scrollBy({ left: direction * el.clientWidth, behavior: "smooth" });
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

onUnmounted(() => {
    window.removeEventListener('resize', updateScroll);
});

watch(courses, () => {
    nextTick(() => updateScroll());
});

const refreshCourses = async () => {
    try {
        const refreshed = await $fetch<Course[]>(getBaseUrl() + "/api/v2/me/courses?limit=4");
        _courses.value = refreshed;
        coursesCache.value = refreshed;
    } catch {}
};

const selectedDeleteCourse = ref<Course | null>(null);

const openDelete = (course: Course) => {
    selectedDeleteCourse.value = course;
    enabledModal.value = "deleteCourse";
};

const deleteCourse = async () => {
    if (!selectedDeleteCourse.value) return;
    
    isActionInProgress.value = true;

    try {
        await $fetch(getBaseUrl() + `/api/v2/courses/${selectedDeleteCourse.value.uuid}`, {
            method: "DELETE"
        });

        enabledModal.value = null;
        selectedDeleteCourse.value = null;

        push.success({
            title: "Kurz smazán",
            message: "Kurz byl úspěšně smazán.",
            duration: 5000
        });

        await refreshCourses();
    } catch (err) {
        push.error({
            title: "Chyba při mazání kurzu",
            message: "Nepodařilo se smazat kurz. Zkuste to prosím znovu.",
            duration: 5000
        })
    } finally {
        isActionInProgress.value = false;
    }
};
</script>

<template>
    <Head>
        <Title>Přehled • Think different Academy</Title>
    </Head>

    <section>

        <h1 :class="$style.nadpis">Přehled</h1>
        <p :class="$style.podnapis">Vítej zpět, <strong>{{ loggedAccount?.fullNameWithoutTitles }}</strong>! Zde najdeš přehled svých kurzů a můžeš spravovat svůj obsah.</p>

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
                <button :class="['liquid-glass', $style.leftBtn, canScrollLeft && $style.active]" @click="scroll(-1)"><span><</span></button>
                <button :class="['liquid-glass', $style.rightBtn, canScrollRight && $style.active]" @click="scroll(1)"><span>></span></button>

                <ul ref="courseList" @scroll="updateScroll">
                    <li v-for="course in courses" :key="course.uuid">
                        <CourseCard
                            :course="course"
                            edit-mode
                            @delete="openDelete(course)"
                        />
                    </li>
                </ul>
            </div>
        </div>
    </section>

    <Teleport to="#teleports">
        <!-- CREATE -->
        <Modal 
            :enabled="enabledModal === 'createCourse'" 
            @close="enabledModal = null" 
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
            :class="$style.createCourseModal"
        >
            <h3>Vytvořit nový kurz</h3>
            <CourseForm
                mode="create"
                @finished="() => { enabledModal = null; refreshCourses(); }"
                :categories="categories || []"
            />
        </Modal>

        <!-- DELETE -->
        <ModalDestructive
            :enabled="enabledModal === 'deleteCourse'"
            @close="enabledModal = null"
            :yes-action="deleteCourse"
            :description="`Opravdu si přeješ smazat kurz „${ selectedDeleteCourse?.name }”?`"
        />
    </Teleport>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.createCourseModal {
    h3 {
        margin-top: 0;
        margin-bottom: 32px;
    }
}

.nadpis {
    font-size: 64px;
    margin: 0;
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    //max-width: 700px;
    color: var(--text-color-secondary);
}

.actionButtons {
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
    margin-top: calc(64px + 12px);
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

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
    section {
        margin-top: -50px;
    }
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
    .actionButtons {
        flex-direction: column;
        align-items: stretch;
        
        .createCourse {
            button {
                padding: 4px 16px;
                
                p {
                    font-size: 20px;
                }
                
                .icon {
                    width: 32px;
                    height: 32px;
                }
            }
        }
    }
    
    .courses {
        .header {
            flex-direction: column;
            align-items: flex-start;
            gap: 4px;
        }
    }
}
</style>
