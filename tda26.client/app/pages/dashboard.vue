<script setup lang="ts">
import { Head, Title } from '#components';
import type { Account, Course } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import CourseCard from "~/components/pagespecific/CourseCard.vue";
import { NuxtLink } from "#components";
import Button from "~/components/Button.vue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>('loggedAccount');
        if (!user.value) return navigateTo('/login');
    }
});

const user = useState<Account | null>('loggedAccount');

const { data: _courses, pending: coursesPending } = await useFetch<Course[]>(getBaseUrl() + '/api/v2/me/courses?max=4', {
    server: false
});

const courses = computed(() => {
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

</script>

<template>
    <Head>
        <Title>Dashboard • Think different Academy</Title>
    </Head>

    <h1 :class="$style.nadpis">Přehled</h1>
    <p :class="$style.podnapis">Lorem ipsum dolor sit amet, consectetur adipisicing elit. Adipisci at commodi corporis, delectus ducimus est facilis ipsum molestiae nisi odit, reprehenderit repudiandae sapiente sit temporibus voluptates. Consectetur cumque esse itaque.</p>

    <div :class="$style.actionButtons">
        <NuxtLink to="/courses/add" :class="$style.createCourse">
            <Button button-style="primary" accent-color="primary"><span :class="$style.icon"></span><p>Vytvořit nový kurz</p></Button>
        </NuxtLink>
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
                    <CourseCard :course="course!" :edit-mode="true" />
                </li>
            </ul>
        </div>
    </div>
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
        
        li >* {
            box-shadow: none;
            width: 400px;
            height: 400px;
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
