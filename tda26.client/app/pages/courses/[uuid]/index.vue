<script setup lang="ts">
import type {
    Account,
    Course,
    CourseCategory, CourseStatus,
    Material,
    Quiz
} from "#shared/types";
import formatCzechCount  from "#shared/utils/formatCzechCount";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import MaterialItem from "~/components/courses/[uuid]/MaterialItem.vue";
import Modal from "~/components/Modal.vue";
import ModalDestructive from "~/components/ModalDestructive.vue";
import MaterialFormItem from "~/components/pagespecific/MaterialFormItem.vue";
import { ref } from "vue";
import QuizItem from "~/components/courses/[uuid]/QuizItem.vue";
import { ClientOnly, NuxtLink, Head, Title } from "#components";
import NumberExponential from "~/components/NumberExponential.vue";
import Avatar from "~/components/Avatar.vue";
import Input from "~/components/Input.vue";
import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
import LoginForm from "~/components/LoginForm.vue";
import RegisterForm from "~/components/RegisterForm.vue";
import CategoryAndTagsSelection from "~/components/pagespecific/CategoryAndTagsSelection.vue";
import timeAgoString from "#shared/utils/timeAgoString";
import { statusToText } from "#shared/utils/statusMapper";

import { useCourseDialogs } from "~/composables/courses/[uuid]/useCourseDialogs";
import { useCourseEdit } from "~/composables/courses/[uuid]/useCourseEdit";
import { useCourseMaterials } from "~/composables/courses/[uuid]/useCourseMaterials";
import { useCourseQuizzes } from "~/composables/courses/[uuid]/useCourseQuizzes";
import { useCourseFeed } from "~/composables/courses/[uuid]/useCourseFeed";
import { useCourseRating } from "~/composables/courses/[uuid]/useCourseRating";
import { useCourseDelete } from "~/composables/courses/[uuid]/useCourseDelete";
import { useCourseViewEvent } from "~/composables/courses/[uuid]/useCourseViewEvent";
import { useBeforeUnloadUnsavedChanges } from "~/composables/courses/[uuid]/useBeforeUnloadUnsavedChanges";
import { useModuleVisibility } from "~/composables/courses/[uuid]/useModuleVisibility";
import CourseCardImageContainer from "~/components/pagespecific/CourseCardImageContainer.vue";
import useCoursePublicationSchedule from "~/composables/courses/[uuid]/useCoursePublicationSchedule";
import useCourseSSE from "~/composables/courses/[uuid]/useCourseSSE";
import ContextMenuButton from "~/components/contextmenu/ContextMenuButton.vue";
import ContextMenu from "~/components/contextmenu/ContextMenu.vue";
import {useContextMenu} from "~/composables/useContextMenu";
import {useCourseStatus} from "~/composables/courses/[uuid]/useCourseStatus";
import Popover from "~/components/Popover.vue";


definePageMeta({
    layout: "normal-page-layout",
    alias: ["/course/:uuid"],
    middleware: [
        defineNuxtRouteMiddleware(async (to) => {
            const uuid = to.params.uuid as string;
            const isEditMode = to.query.edit === "true";

            // pokud chybi uuid
            if (!uuid) {
                return navigateTo("/courses");
            }

            // pokud je stranka /course/:uuid, perm presmeruje na /courses/:uuid
            if (to.path.startsWith("/course/")) {
                return navigateTo(`/courses/${uuid}`);
            }

            try {
                const course = await $fetch<Course>(`${getBaseUrl()}/api/v1/courses/${uuid}`, {
                    query: { full: false },
                    headers: {
                        'Cookie': useRequestHeaders(['cookie']).cookie || ''
                    }
                });

                if (!course) {
                    return navigateTo("/courses");
                }
                
                if (isEditMode)        {        
                    // pokud je edit mode, musi byt kurz draft
                    if (course.status !== "draft") {
                        return navigateTo(`/courses/${uuid}`);
                    }
                }
                
                const courseSmallState = useState<Course | null>(`course-small-${uuid}`, () => null);
                courseSmallState.value = course;

                const loggedAccount = useState<Account | null>("loggedAccount");

                const isAdmin = loggedAccount.value?.type === "admin";
                const isAuthor = loggedAccount.value?.uuid === course.account?.uuid;
                
                // pokud se jedna o draft/scheduled/archived kurz a uzivatel neni admin ani autor, presmeruje na /courses
                if (["draft", "scheduled", "archived"].includes(course.status) && !(isAdmin || isAuthor)) {
                    return navigateTo("/courses");
                }
            } catch (e) {
                console.error("Error loading small course:", e);
                return navigateTo("/courses");
            }
        })
    ]
});

const loggedAccount = useState<Account | null>("loggedAccount");
const route = useRoute();
const uuid = route.params.uuid as string;
const isEditMode = route.query.edit === "true";

const isActionInProgress = ref(false);

// modaly + sdilene errory
const {enabledModal, authTab, updateError, deleteError, feedPostError} = useCourseDialogs();

// server small fetch
const courseSmallState = useState<Course | null>(`course-small-${uuid}`);

const _courseSmall = ref<Course | null>(courseSmallState.value);
const courseSmallError = ref<Error | null>(null);

if (!_courseSmall.value) {
    console.error("Error loading small course: missing state from middleware");
    courseSmallError.value = new Error("Course not loaded");
    await navigateTo("/courses");
}

const courseSmall = ref<Course>(_courseSmall.value!);

// pokud je edit mode, musi byt prihlasen uzivatel a vlastnik kurzu
if (isEditMode) {
    if (loggedAccount.value?.type !== "admin" && (!loggedAccount.value || loggedAccount.value.uuid !== courseSmall.value.account?.uuid)) {
        await navigateTo(`/courses/${uuid}`);
    }
}

// client full fetch
const { data: _course, pending: coursePending, error: courseError } = useFetch<Course>(() => getBaseUrl() + `/api/v1/courses/${uuid}`, {
    query: { full: true },
    server: false,
    key: `course-${uuid}-full`,
});

if (courseError.value) {
    console.error("Error loading full course:", courseError.value);
}

const course = ref<Course | null>(_course.value ?? null);
const originalCourse = ref<Course | null>(null);

// categories jsou server-renderovane, aby select mel data hned
const { data: categories } = await useFetch<CourseCategory[]>(
    getBaseUrl() + "/api/v1/course-categories",
    { server: true }
);

// view event (recaptcha)
useCourseViewEvent(uuid);

// edit stav + ulozeni
const {statusOptions, editedCategoryUuid, editedTagsUuid, isDirty, ownsCourse, saveCourseChanges, clearCourseCaches, updateCourseTitle, updateCourseDescription} = useCourseEdit({uuid, isEditMode, courseSmall, course, originalCourse, courseData: _course, categories, loggedAccount, isActionInProgress,});

// upozorneni pri zavirani tab
useBeforeUnloadUnsavedChanges(isDirty);

// rating
const {ratingLoading, isThisCourseLikedDesign, isThisCourseDislikedDesign, optimisticLikeCount, addRating,} = useCourseRating({uuid, loggedAccount, courseSmall, course, enabledModal,});

// menu
const menuItems = ["Materiály", "Kvízy", "Aktivita"];
const selectedItem = ref(menuItems[0]);

const selectItem = (item: string) => {
    selectedItem.value = item;
};

// publication schedule
const { cancelPublicationSchedule, formattedPublishTime, selectedTimeOption, timeOptions, customDateTime, maxCustomDatetime, minCustomDatetime, finalDateTime, confirmPublicationSchedule } = useCoursePublicationSchedule({ enabledModal, course: courseSmall, originalCourse, updateError, deleteError });

// materiály
const {selectedMaterial, editingMaterial, openCreateMaterialModal, openUpdateMaterialModal, openDeleteMaterialModal, handleMaterialCreate, handleMaterialUpdate, handleMaterialDelete} = useCourseMaterials({course, enabledModal, isActionInProgress, updateError, deleteError,});

// kvízy
const { selectedQuiz, handleQuizCreate, handleQuizDelete } = useCourseQuizzes({ course, enabledModal, isActionInProgress, updateError, deleteError });

// status kurzu
const { updateCourseStatus, isLoading: isCourseStatusLoading, currentStatus } = useCourseStatus({ course: courseSmall });

// context menu
const { isOpen: isContextMenuOpen, position: contextMenuPosition, open: openContextMenu, close: closeContextMenu } = useContextMenu();

// visibility toggling
const moduleLoadingStates = ref<Record<string, boolean>>({});

const handleMaterialVisibilityToggle = async (material: Material) => {
    moduleLoadingStates.value[material.uuid] = true;
    try {
        const moduleRef = ref(material);
        const { changeVisibility } = useModuleVisibility({ courseUuid: uuid, module: moduleRef });
        await changeVisibility(!material.isVisible);
    } finally {
        moduleLoadingStates.value[material.uuid] = false;
    }
};

const handleQuizVisibilityToggle = async (quiz: Quiz) => {
    moduleLoadingStates.value[quiz.uuid] = true;
    try {
        const moduleRef = ref(quiz);
        const { changeVisibility } = useModuleVisibility({ courseUuid: uuid, module: moduleRef });
        await changeVisibility(!quiz.isVisible);
    } finally {
        moduleLoadingStates.value[quiz.uuid] = false;
    }
};

// feed
const { selectedFeedFilter, feedData, feedPending, feedError, feedPosts, selectedFeedPost, editingFeedPost, openCreateFeedPost, openUpdateFeedPost, openDeleteFeedPost, handleFeedPostCreate, handleFeedPostUpdate, handleFeedPostDelete } = useCourseFeed({ uuid, course, enabledModal, isActionInProgress, feedPostError });

// delete course
const {openDeleteCourseModal, handleCourseDelete,} = useCourseDelete({courseSmall, enabledModal, isActionInProgress, deleteError, clearCourseCaches,});

// obecne sse
const { } = useCourseSSE({ course: courseSmall, courseFullData: course, editMode: isEditMode });

function editBackClick() {
    window.location.href = `/courses/${courseSmall.value?.uuid}`;
}

async function saveCourseAndExit() {
    await saveCourseChanges();
    editBackClick();
}

function editClick() {
    window.location.href = `/courses/${courseSmall.value?.uuid}?edit=true`;
}

function handleAuthSuccess() {
    enabledModal.value = null;
    window.location.reload();
}

const contextMenuItems = computed(() => {
    return [
        {
            text: "Upravit kurz",
            onClick: editClick,
            iconPath: "/icons/pen.svg",
            disabled: courseSmall.value.status !== "draft",
        },
        {
            text: "Smazat kurz",
            onClick: openDeleteCourseModal,
            iconPath: "/icons/trash.svg",
            disabled: courseSmall.value.status !== "draft",
        }
    ];
});
</script>

<template>
    <Head>
        <Title>{{ courseSmall?.name ?? "Kurz" }} • Think different Academy</Title>
    </Head>

    <div :class="[$style.course, isEditMode && $style.editMode]">
        <div :class="$style.basic">
            <div :class="$style.left">
                <div :class="$style.categoryAndTags">
                    <Input
                        v-if="isEditMode && course"
                        :key="course?.tags?.length"
                        v-model="editedCategoryUuid"
                        type="select"
                        :class="$style.editableInput"
                    >
                        <option
                            v-for="cat in categories"
                            :key="cat.uuid"
                            :value="cat.uuid"
                        >
                            {{ cat.label }}
                        </option>
                    </Input>

                    <p v-else-if="courseSmall.category" :class="$style.category">{{ courseSmall.category.label }}</p>

                    <template v-if="isEditMode && course" :class="$style.tags">
                        <CategoryAndTagsSelection
                            v-if="editedCategoryUuid && isEditMode"
                            :key="editedCategoryUuid"
                            v-model="editedTagsUuid"
                            :category-uuid="editedCategoryUuid"
                        />
                    </template>
                </div>

                <h1
                    :class="['text-gradient', $style.title, $style.editable]"
                    :contenteditable="isEditMode"
                    @input="(e) => updateCourseTitle((e.target as HTMLElement).innerText.trim())"
                >{{ courseSmall?.name }}</h1>

                <p
                    :class="[$style.editable]"
                    :contenteditable="isEditMode"
                    @input="(e) => updateCourseDescription((e.target as HTMLElement).innerText.trim())"
                >{{ courseSmall?.description }}</p>
            </div>

            <div :class="['liquid-glass',$style.right]">
                <CourseCardImageContainer :course="courseSmall" :class="$style.image" />

                <p v-if="currentStatus" :class="$style.status" :data-status="currentStatus">{{ statusToText(currentStatus) }}</p>

                <div :class="$style.fields">
                    <div :class="$style.el">
                        <NumberExponential :value="courseSmall?.viewCount ?? 0" :container-class="$style.nexp" :numberClass="$style.item" />
                        <p :class="$style.title">{{ formatCzechCount(courseSmall?.viewCount ?? 0, {one: "Zhlédnutí", few: "Zhlédnutí", many: "Zhlédnutí"} ) }}</p>
                    </div>
                    <div :class="$style.el">
                        <NumberExponential :value="course?.materials?.length ?? 0" :container-class="$style.nexp" :numberClass="$style.item" />
                        <p :class="$style.title">{{ formatCzechCount(course?.materials?.length ?? 0,  { one: "Materiál", few: "Materiály", many: "Materiálů" } ) }}</p>
                    </div>
                    <div :class="$style.el">
                        <NumberExponential :value="course?.quizzes?.length ?? 0" :container-class="$style.nexp" :numberClass="$style.item" /> <!-- TODO: dodělat recenze -->
                        <p :class="$style.title">{{ formatCzechCount(course?.quizzes?.length ?? 0, {one: "Kvíz", few: "Kvízy", many: "Kvízů"} ) }}</p>
                    </div>
                </div>

                <div :class="$style.authorAndRating">
                    <NuxtLink v-if="courseSmall?.account" :class="[$style.author, { [$style.clickable]: courseSmall.lecturer }]" :to="courseSmall?.lecturer ? `/lecturer/${courseSmall?.lecturer?.uuid}` : '' ">
                        <Avatar :class="$style.avatar" :letter-style="{ color: 'var(--accent-color-secondary-theme-text)' }" :name="courseSmall?.lecturer?.fullName ?? courseSmall?.account?.fullName ?? '?'" :src="courseSmall?.lecturer?.pictureUrl ?? null" />
                        <p>{{ courseSmall?.lecturer?.fullName ?? courseSmall?.account?.fullName }}</p>
                    </NuxtLink>

                    <div :class="$style.rating">
                        <!-- like a dislike button -->
                        <div :class="[$style.duo, { [$style.active]: isThisCourseLikedDesign  }]" @click="addRating('like')">
                            <div :class="$style.icon"/>
                            <p>{{ optimisticLikeCount }}</p>
                        </div>

                        <div :class="[$style.duo, { [$style.active]: isThisCourseDislikedDesign }]" @click="addRating('dislike')">
                            <div :class="$style.icon" style="rotate: 180deg"/>
                            <p>Nelíbí se</p>
                        </div>
                    </div>
                </div>

                <div v-if="ownsCourse && !isEditMode" :class="$style.courseActions">
                    <!-- Primary button -->
                    <Button
                        v-if="courseSmall?.status === 'draft' || courseSmall?.status === 'scheduled' || courseSmall?.status === 'paused'"
                        button-style="primary"
                        accent-color="secondary"
                        :loading="isCourseStatusLoading"
                        @click="updateCourseStatus('live')"
                    >Spustit</Button>
                    <Button
                        v-if="courseSmall?.status === 'live'"
                        button-style="primary"
                        accent-color="secondary"
                        :loading="isCourseStatusLoading"
                        @click="updateCourseStatus('paused')"
                    >Pozastavit</Button>
                    <Button
                        v-if="courseSmall?.status === 'archived'"
                        button-style="primary"
                        accent-color="secondary"
                        :loading="isCourseStatusLoading"
                        @click="updateCourseStatus('draft')"
                    >Obnovit na návrh</Button>
                    
                    <!-- Secondary button -->
                    <Button
                        v-if="courseSmall?.status === 'draft'"
                        button-style="secondary"
                        accent-color="secondary"
                        :loading="isCourseStatusLoading"
                        @click="enabledModal = 'schedulePublication'"
                    >Naplánovat</Button>
                    <Button
                        v-if="courseSmall?.status === 'scheduled'"
                        button-style="secondary"
                        accent-color="secondary"
                        :loading="isCourseStatusLoading"
                        @click="updateCourseStatus('draft')"
                    >Vrátit</Button>
                    <Button
                        v-if="courseSmall?.status === 'live'"
                        button-style="secondary"
                        accent-color="secondary"
                        :loading="isCourseStatusLoading"
                        @click="updateCourseStatus('archived')"
                    >Ukončit</Button>

                    <div>
                        <ContextMenuButton @open="openContextMenu"/>
                        <ContextMenu
                            :items="contextMenuItems"

                            :visible="isContextMenuOpen"
                            :x="contextMenuPosition.x"
                            :y="contextMenuPosition.y"
                            @close="closeContextMenu"
                        />
                    </div>
                </div>
            </div>
        </div>

        <!-- moduly (kvizy, aktivita, materialy..) -->
        <div :class="$style.modules">
            <nav>
                <ul>
                    <li
                        v-for="item in menuItems"
                        :key="item"
                        :class="[item === selectedItem ? $style.active : '']"
                        @click="selectItem(item)">
                        {{ item }}
                    </li>
                </ul>
            </nav>

            <div :class="['liquid-glass']" style="overflow-x: auto; overflow-y: hidden;">
                <SmoothSizeWrapper :change-width="false" v-if="courseSmall?.account?.uuid === loggedAccount?.uuid || courseSmall.status === 'live'">
                    <ClientOnly>
                        <div v-if="selectedItem == 'Materiály'" :class="$style.materials">
                            <Popover teleport :disabled="courseSmall.status === 'draft'" v-if="ownsCourse">
                                <template #trigger>
                                    <Button
                                        button-style="primary"
                                        accent-color="primary"
                                        :class="$style.addMaterialButton"
                                        @click="openCreateMaterialModal"
                                        :disabled="courseSmall.status !== 'draft'"
                                    >
                                        Přidat nový materiál
                                    </Button>
                                </template>

                                <template #content>Kurz musí být návrh</template>
                            </Popover>

                            <p v-if="coursePending">Načítání materiálů...</p>
                            <p v-else-if="course?.materials === undefined || course?.materials.length == 0">Tento kurz nemá žádné materiály.</p>
                            <ul v-else>
                                <li v-for="material in course?.materials" :key="material.uuid">
                                    <MaterialItem
                                        v-if="ownsCourse || material.isVisible"
                                        :material="material"
                                        :course="course"
                                        :edit-mode="ownsCourse"
                                        :is-visibility-toggle-loading="moduleLoadingStates[material.uuid] || false"
                                        @edit="openUpdateMaterialModal"
                                        @delete="openDeleteMaterialModal"
                                        @toggle-visibility="handleMaterialVisibilityToggle"
                                    />
                                </li>
                            </ul>
                        </div>
                        <div v-if="selectedItem == 'Kvízy'" :class="$style.materials">
                            <Button v-if="ownsCourse" button-style="primary" accent-color="primary" :class="$style.addMaterialButton" @click="enabledModal = 'createQuiz'">
                                Přidat nový kvíz
                            </Button>

                            <p v-if="coursePending">Načítání kvízů...</p>
                            <p v-else-if="course?.quizzes === undefined || course?.quizzes.length == 0">Tento kurz nemá žádné kvízy.</p>
                            <ul v-else>
                                <li v-for="quiz in course?.quizzes" :key="quiz.uuid">
                                    <QuizItem
                                        v-if="ownsCourse || quiz.isVisible"
                                        :quiz="quiz"
                                        :course="course"
                                        :edit-mode="ownsCourse"
                                        :is-visibility-toggle-loading="moduleLoadingStates[quiz.uuid] || false"
                                        @delete="(q) => { selectedQuiz = q; enabledModal = 'deleteQuiz'; }"
                                        @toggle-visibility="handleQuizVisibilityToggle"
                                    />
                                </li>
                            </ul>
                        </div>
                        <div v-if="selectedItem == 'Aktivita'" :class="$style.activity">
                            <div v-if="feedData" :class="$style.activityHeader">
                                <Button
                                    v-if="ownsCourse"
                                    button-style="primary"
                                    accent-color="primary"
                                    :class="$style.addFeedPost"
                                    @click="openCreateFeedPost"
                                >
                                    Přidat příspěvek
                                </Button>

                                <div :class="$style.feedPostFilter">
                                    <!--                                    <p :class="$style.feedFilterLabel">Filtr:</p>-->
                                    <!--                                <Input-->
                                    <!--                                    placeholder="Hledat v aktivitě..."-->
                                    <!--                                    :disabled="true"-->
                                    <!--                                />-->
                                    <div :class="[$style.feedFilterOptions, 'liquid-glass']">
                                        <div
                                            :class="[$style.feedFilterOption, selectedFeedFilter === 'all' && $style.active]"
                                            @click="selectedFeedFilter = 'all'"
                                        >
                                            <p>Vše</p>
                                        </div>

                                        <div
                                            :class="[$style.feedFilterOption, selectedFeedFilter === 'material' && $style.active]"
                                            @click="selectedFeedFilter = 'material'"
                                        >
                                            <p>Materiály</p>
                                        </div>

                                        <div
                                            :class="[$style.feedFilterOption, selectedFeedFilter === 'quiz' && $style.active]"
                                            @click="selectedFeedFilter = 'quiz'"
                                        >
                                            <p>Kvízy</p>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <ul>
                                <li>
                                    <p v-if="feedPending">Načítání aktivity...</p>
                                    <p v-else-if="!feedData || feedData.length == 0">Tento kurz nemá žádnou aktivitu.</p>
                                    <p v-else-if ="feedError" >Nepodařilo se načíst aktivitu kurzu. Zkuste to prosím znovu.</p>
                                </li>

                                <li v-for=" feedPost in feedPosts" v-if="feedData" :key="feedPost.uuid">
                                    <div :class="$style.feedPostWrapper">
                                        <div
                                            :class="$style.feedPostLeft"
                                            :style="{ borderLeft: `8px solid var(${feedPost.background})` }"
                                        >
                                            <div
                                                :class="$style.iconWrapper"
                                                :style="{ backgroundColor: `var(${feedPost.background})` }"
                                            >
                                                <div
                                                    :class="$style.feedPostIcon"
                                                    :style="{ maskImage: `url(${feedPost.icon})` }"
                                                />
                                            </div>
                                        </div>
                                        <div :class="$style.feedPostRight">
                                            <div
                                                v-if="ownsCourse"
                                                :class="$style.feedPostActions"
                                            >
                                                <Button
                                                    button-style="secondary"
                                                    accent-color="secondary"
                                                    v-if="feedPost.type === 'manual'"
                                                    @click="openUpdateFeedPost(feedPost)"
                                                >
                                                    Upravit
                                                </Button>

                                                <Button
                                                    button-style="secondary"
                                                    accent-color="secondary"
                                                    :style="{ '--color': 'var(--color-error)' }"
                                                    @click="openDeleteFeedPost(feedPost)"
                                                >
                                                    Smazat
                                                </Button>
                                            </div>
                                            <div :class="$style.feedPostHeader">
                                                <div
                                                    :class="$style.feedPurpose"
                                                    :style="{
                                                              backgroundColor: `rgb(from var(${feedPost.background}) r g b / 0.15)`
                                                            }"
                                                >
                                                    <p :style="{ color: `var(${feedPost.color})` }">
                                                        {{ feedPost.purposeLabel }}
                                                    </p>
                                                </div>
                                                <div :class="$style.feedTimestamp">{{ timeAgoString(feedPost.createdAt) }}</div>
                                            </div>
                                            <div v-if="feedPost.author !== null " :class="$style.feedPostAuthor">
                                                <Avatar
                                                    :class="$style.feedAvatar"
                                                    :letter-style="{ color: 'var(--accent-color-secondary-theme-text)' }"
                                                    :name="feedPost.author?.fullName ?? '?'"
                                                    :src="feedPost.author?.pictureUrl ?? null"
                                                    :size="32"
                                                />
                                                <p :class="$style.authorName">{{ feedPost.author?.fullName }}</p>
                                            </div>
                                            <div :class="$style.feedPostContent">
                                                <p> {{ feedPost.message }} </p>
                                            </div>
                                        </div>
                                    </div>

                                    <!--                                    <FeedPostItem-->
                                    <!--                                        :feed-post="feedPost"-->
                                    <!--                                        :edit-mode="ownsCourse"-->
                                    <!--                                        @edit="openEditFeedPost"-->
                                    <!--                                        @delete="openDeleteFeedPost"-->
                                    <!--                                    />-->
                                </li>
                            </ul>
                        </div>
                    </ClientOnly>
                </SmoothSizeWrapper>

                <SmoothSizeWrapper :change-width="false" v-else>
                    <p>Kurz je momentálně ve stavu {{ statusToText(courseSmall?.status ?? "unknown") }}. Moduly lze vidět pouze pokud kurz probíhá.</p>
                </SmoothSizeWrapper>
            </div>
        </div>
    </div>


    <Teleport to="#teleports">
        <!-- Edit controls -->
        <div v-if="isEditMode" :class="$style.editControls">
            <div :class="$style.top">
                <Button
                    button-style="secondary"
                    :style="{ '--color': 'var(--color-error)' }"
                    :disabled="isActionInProgress"
                    @click="openDeleteCourseModal"
                >
                    Smazat kurz
                </Button>
            </div>

            <div :class="$style.bottom">
                <Button
                    button-style="primary"
                    :disabled="!isDirty || isActionInProgress"
                    @click="saveCourseChanges"
                >
                    Uložit změny
                </Button>
                <Button
                    button-style="secondary"
                    :disabled="!isDirty || isActionInProgress"
                    @click="saveCourseAndExit()"
                >
                    Uložit a ukončit úpravy
                </Button>
                <Button
                    button-style="tertiary"
                    :disabled="isActionInProgress"
                    @click="editBackClick"
                >
                    Ukončit úpravy
                </Button>
            </div>
        </div>





        <!-- SCHEDULE PUBLICATION -->
        <Modal
            :enabled="enabledModal === 'schedulePublication'"
            @close="cancelPublicationSchedule"
            can-be-closed-by-clicking-outside
            title="Naplánovat publikaci"
            :modalStyle="{ maxWidth: '800px' }"
            :class="$style.schedulePublicationModal"
        >
            <div :class="$style.modalContent">

                <label>Čas publikace</label>

                <div v-if="formattedPublishTime" :class="$style.scheduledInfo">
                    <p>Publikace proběhne:</p>
                    <p>{{ formattedPublishTime }}</p>
                </div>

                <Input type="select" v-model="selectedTimeOption">
                    <option
                        v-for="option in timeOptions"
                        :key="option.label"
                        :value="option.values"
                    >
                        {{ option.label }}
                    </option>
                </Input>

                <div v-if="selectedTimeOption === 'custom'" :class="$style.customDateWrapper">
                    <Input
                        type="datetime-local"
                        v-model="customDateTime"
                        :maxDate="maxCustomDatetime"
                        :minDate="minCustomDatetime"
                    >
                    </Input>
                </div>

                <div :class="$style.modalButtons">
                    <Button button-style="tertiary" @click="cancelPublicationSchedule">
                        Zrušit
                    </Button>

                    <Button
                        button-style="primary"
                        accent-color="secondary"
                        :disabled="!finalDateTime"
                        @click="confirmPublicationSchedule"
                    >
                        Naplánovat
                    </Button>
                </div>

            </div>
        </Modal>



        <!-- CREATE -->
        <Modal
            :enabled="enabledModal === 'createMaterial'"
            can-be-closed-by-clicking-outside
            :modal-style="{ maxWidth: '800px' }"
            :class="$style.createMaterialModal"
            @close="enabledModal = null"
        >
            <h3>Vytvoření nového materiálu</h3>
            <MaterialFormItem
                v-model="editingMaterial"
                :index="0"
                :show-remove-button="false"
                @file-selected="(_, file) => editingMaterial.file = file"
            />
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" :disabled="isActionInProgress" @click="enabledModal = null">Zrušit</Button>

                <Button
                    button-style="primary"
                    accent-color="secondary"
                    :disabled="isActionInProgress"
                    @click="handleMaterialCreate"
                >
                    Vytvořit materiál
                </Button>
            </div>
            <p v-if="updateError" class="error-text">{{ updateError }}</p>
        </Modal>

        <!-- UPDATE -->
        <Modal
            :enabled="enabledModal === 'updateMaterial'"
            can-be-closed-by-clicking-outside
            :modal-style="{ maxWidth: '800px' }"
            :class="$style.updateMaterialModal"
            @close="enabledModal = null"
        >
            <h3>Úprava materiálu</h3>

            <MaterialFormItem
                v-if="editingMaterial"
                v-model="editingMaterial"
                :index="0"
                :show-remove-button="false"
                @file-selected="(_, file) => editingMaterial.file = file"
            />


            <div :class="$style.modalButtons">
                <Button button-style="tertiary" :disabled="isActionInProgress" @click="enabledModal = null">Zrušit</Button>

                <Button
                    button-style="primary"
                    accent-color="secondary"
                    :disabled="isActionInProgress"
                    @click="handleMaterialUpdate"
                >
                    Uložit změny
                </Button>
            </div>

            <p v-if="updateError" class="error-text">{{ updateError }}</p>
        </Modal>

        <!-- DELETE MATERIAL -->
        <ModalDestructive
            :enabled="enabledModal === 'deleteMaterial'"
            can-be-closed-by-clicking-outside
            title="Potvrzení akce"
            description="Opravdu chceš smazat tento materiál? Tuto akci nelze vrátit zpět."
            :yes-action="handleMaterialDelete"
            @close="enabledModal = null"
        >
            <h3>Opravdu si přeješ smazat materiál <i class="text-gradient">{{ selectedMaterial?.name }}</i>?</h3>
            <p>Tuto akci nelze vrátit zpět.</p>
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" :disabled="isActionInProgress" @click="enabledModal = null">Zrušit</Button>
                <Button
                    button-style="primary"
                    accent-color="secondary"
                    :disabled="isActionInProgress"
                    @click="handleMaterialDelete"
                >
                    Smazat materiál
                </Button>
            </div>
            <p v-if="deleteError" class="error-text">{{ deleteError }}</p>
        </ModalDestructive>

        <!-- DELETE QUIZ -->
        <ModalDestructive
            :enabled="enabledModal === 'deleteQuiz'"
            can-be-closed-by-clicking-outside
            title="Potvrzení akce"
            description="Opravdu chceš smazat tento kvíz? Tuto akci nelze vrátit zpět."
            :yes-action="handleQuizDelete"
            @close="enabledModal = null"
        >
            <h3>Opravdu si přeješ smazat kvíz <i class="text-gradient">{{ selectedQuiz?.title }}</i>?</h3>
            <p>Tuto akci nelze vrátit zpět.</p>
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" :disabled="isActionInProgress" @click="enabledModal = null">Zrušit</Button>
                <Button
                    button-style="primary"
                    accent-color="secondary"
                    :disabled="isActionInProgress"
                    @click="handleQuizDelete"
                >
                    Smazat kvíz
                </Button>
            </div>
            <p v-if="deleteError" class="error-text">{{ deleteError }}</p>
        </ModalDestructive>

        <!-- CREATE QUIZ -->
        <Modal
            :enabled="enabledModal === 'createQuiz'"
            can-be-closed-by-clicking-outside
            :modal-style="{ maxWidth: '800px' }"
            :class-name="$style.createQuizModal"
            @close="enabledModal = null"
        >
            <h3>Vytvoření nového kvízu</h3>
            <form
                @submit.prevent="handleQuizCreate"
            >
                <label for="createQuizName">Název kvízu</label>
                <Input
                    id="createQuizName"
                    name="createQuizName"
                    max="128"
                    required
                    :disabled="isActionInProgress"
                />

                <div :class="$style.modalButtons">
                    <Button type="reset" button-style="tertiary" :disabled="isActionInProgress" @click="enabledModal = null">Zrušit</Button>

                    <Button
                        button-style="primary"
                        accent-color="secondary"
                        type="submit"
                        :disabled="isActionInProgress"
                    >
                        Vytvořit kvíz
                    </Button>
                </div>
            </form>
            <p v-if="updateError" class="error-text">{{ updateError }}</p>
        </Modal>

        <!-- LOGIN REQUIRED MODAL -->
        <Modal
            :enabled="enabledModal === 'loginRequired'"
            can-be-closed-by-clicking-outside
            :modal-style="{ maxWidth: '500px' }"
            @close="enabledModal = null"
        >
            <SmoothSizeWrapper>
                <div :class="$style.authModalHeader">
                    <h3>Pro hodnocení kurzu se musíš přihlásit.</h3>

                    <div :class="$style.authTabs">
                        <button
                            :class="[$style.authTab, { [$style.active]: authTab === 'login' }]"
                            type="button"
                            @click="authTab = 'login'"
                        >
                            Přihlášení
                        </button>

                        <button
                            :class="[$style.authTab, { [$style.active]: authTab === 'register' }]"
                            type="button"
                            @click="authTab = 'register'"
                        >
                            Registrace
                        </button>
                    </div>
                </div>

                <div :class="$style.authFormContainer">
                    <Transition
                        mode="out-in"
                        :enter-active-class="$style.fadeEnterActive"
                        :enter-from-class="$style.fadeEnterFrom"
                        :enter-to-class="$style.fadeEnterTo"
                        :leave-active-class="$style.fadeLeaveActive"
                        :leave-from-class="$style.fadeLeaveFrom"
                        :leave-to-class="$style.fadeLeaveTo"
                    >
                        <div :key="authTab">
                            <LoginForm
                                v-if="authTab === 'login'"
                                @login-success="handleAuthSuccess"
                            />
                            <RegisterForm
                                v-else
                                @register-success="handleAuthSuccess"
                            />
                        </div>
                    </Transition>
                </div>
            </SmoothSizeWrapper>
        </Modal>


        <!-- CREATE FEED POST -->
        <Modal
            :enabled="enabledModal === 'createFeedPost'"
            :modal-style="{ maxWidth: '600px' }"
            @close="enabledModal = null"
        >
            <h3>Nový příspěvek do aktivity</h3>

            <form @submit.prevent="handleFeedPostCreate">
                <label for="feedMessage" :class="$style.feedLabel">Text příspěvku *</label>

                <!--                <textarea-->
                <!--                    id="feedMessage"-->
                <!--                    maxlength="1000"-->
                <!--                    required-->
                <!--                />-->

                <textarea
                    v-model="editingFeedPost.message"
                    :class="$style.feedTextarea"
                    rows="5"
                    placeholder="Napiš oznámení pro studenty…"
                    :disabled="isActionInProgress"
                    required
                />

                <div :class="$style.modalButtons">
                    <Button
                        button-style="tertiary"
                        type="button"
                        :disabled="isActionInProgress"
                        @click="enabledModal = null"
                    >
                        Zrušit
                    </Button>

                    <Button
                        button-style="primary"
                        accent-color="secondary"
                        type="submit"
                        :disabled="isActionInProgress"
                    >
                        Publikovat
                    </Button>
                </div>
            </form>

            <p v-if="feedPostError" class="error-text">
                {{ feedPostError }}
            </p>
        </Modal>

        <!-- UPDATE FEED POST -->
        <Modal
            :enabled="enabledModal === 'updateFeedPost'"
            :modal-style="{ maxWidth: '600px' }"
            :class-name="$style.updateFeedPostModal"
            @close="enabledModal = null"
        >
            <h3>Úprava příspěvku</h3>

            <textarea
                v-model="editingFeedPost.message"
                :class="$style.feedTextarea"
                rows="5"
                :disabled="isActionInProgress"
            />

            <div :class="$style.modalButtons">
                <Button button-style="tertiary" @click="enabledModal = null">
                    Zrušit
                </Button>

                <Button
                    button-style="primary"
                    accent-color="secondary"
                    :disabled="isActionInProgress"
                    @click="handleFeedPostUpdate"
                >
                    Uložit změny
                </Button>
            </div>

            <p v-if="feedPostError" class="error-text">{{ feedPostError }}</p>
        </Modal>

        <!-- DELETE FEED POST -->
        <ModalDestructive
            :enabled="enabledModal === 'deleteFeedPost'"
            title="Smazání příspěvku"
            description="Opravdu chceš smazat tento příspěvek? Tuto akci nelze vrátit."
            :yes-action="handleFeedPostDelete"
            @close="enabledModal = null"
        >
            <p>
                Opravdu chceš smazat tento příspěvek?
            </p>
        </ModalDestructive>

        <!-- DELETE COURSE -->
        <ModalDestructive
            :enabled="enabledModal === 'deleteCourse'"
            title="Smazání kurzu"
            :description="`Opravdu chceš smazat kurz ${courseSmall?.name ?? ''}? Tato akce je nevratná.`"
            :yes-action="handleCourseDelete"
            :yes-text="'Smazat kurz'"
            :no-text="'Zrušit'"
            :can-be-closed-by-clicking-outside="!isActionInProgress"
            @close="!isActionInProgress ? enabledModal = null : null"
        >
            <p style="margin-top: 16px; color: var(--text-color-secondary);">
                Budou smazány všechny materiály, kvízy, hodnocení a další data spojená s tímto kurzem.
            </p>
            <p v-if="deleteError" class="error-text" style="margin-top: 16px;">{{ deleteError }}</p>
        </ModalDestructive>

    </Teleport>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.basic {
    display: flex;
    position: relative;
    gap: 64px;
    align-items: start;

    >.left {
        display: flex;
        flex-direction: column;
        gap: 16px;
        flex: 1;

        >.title{
            font-size: 72px;
            margin: 0;
            overflow: visible;
            width: fit-content;
            padding-bottom: 4px;
        }

        >p {
            font-size: 18px;
            margin: 0;
        }

        >.categoryAndTags {
            display: flex;
            margin-bottom: -8px;
            gap: 6px;

            >.editableInput {
                border: 2px dashed var(--text-color);
            }

            >.category {
                font-family: "Dosis", sans-serif;
                background: linear-gradient(340deg, var(--accent-color-primary) -40%, var(--accent-color-secondary-theme) 110%);
                color: var(--accent-color-primary-text);
                width: fit-content;
                padding: 8px 16px;
                border-radius: 24px;
                margin: 0;
                user-select: none;
            }
        }

    }

    >.right {
        width: 30%;
        max-width: 600px;
        min-width: 400px;
        border-radius: 24px;
        box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/.6),0 0 8px #0000000a;
        margin-top: 64px;
        padding: 24px;
        display: flex;
        flex-direction: column;
        gap: 16px;
        position: relative;
        overflow: hidden;

        >.image {
            position: absolute;
            top: 0;
            right: 0;
            width: 56%;
            aspect-ratio: 16/9;
            min-height: unset;
            pointer-events: none;
            border-radius: 0 0 0 24px;
            mask: radial-gradient(circle at top right, rgba(0, 0, 0, 1) 50%, rgba(0, 0, 0, 0) 80%);
        }

        >.fields {
            display: flex;
            flex-direction: column;
            gap: 8px;

            >.el {
                display: flex;
                align-items: end;
                gap: 20px;

                p {
                    margin: 0;
                }

                .title {
                    font-size: 20px;
                    opacity: 0.5;
                }

                .item {
                    font-size: 32px;
                    font-weight: 600;
                }
            }
        }

        >.authorAndRating {
            display: flex;
            flex-wrap: wrap;
            justify-content: space-between;
            align-items: center;
            gap: 16px;

            .author {
                display: flex;
                align-items: center;
                gap: 8px;
                color: unset;
                text-decoration: none;
                transition-duration: 0.3s;

                &:is(.clickable) {
                    &:hover {
                        opacity: 0.5;
                        transition-duration: 0.3s;
                    }
                }


                .avatar {
                    --size: 24px !important;
                }

                p {
                    margin: 0;
                    font-weight: 600;
                    font-size: 16px;
                    color: var(--text-color-secondary);
                }
            }

            .rating {
                display: flex;
                gap: 12px;

                .duo {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    cursor: pointer;
                    user-select: none;
                    padding: 8px 16px;
                    border-radius: 999px;
                    background-color: var(--background-color-3);
                    transition-duration: 0.3s;

                    &:is(.active) .icon {
                        mask-image: url(/icons/thumbs_up_filled.svg);
                    }

                    &:hover {
                        background-color: var(--background-color-primary);
                        transition-duration: 0.3s;
                    }

                    .icon {
                        width: 16px;
                        aspect-ratio: 1/1;
                        background-color: var(--text-color-primary);
                        border-radius: 4px;
                        mask-image: url(/icons/thumbs_up_outline.svg);
                        mask-size: cover;
                        mask-repeat: no-repeat;
                        mask-position: center;
                    }

                    p {
                        margin: 0;
                        font-size: 16px;
                        font-weight: 600;
                        color: var(--text-color-secondary);
                    }
                }
            }
        }

        >.courseActions {
            display: flex;
            gap: 12px;
            flex-wrap: wrap;
            align-items: center;

            button {
                flex: 1;
                min-width: 140px;
            }
        }
    }
}


.schedulePublicationModal {
    .modalContent {
        display: flex;
        flex-direction: column;
        gap: 24px;

        .customDateWrapper {
            display: flex;
            gap: 16px;

            input {
                flex: 1;
            }
        }

        .modalButtons {
            display: flex;
            gap: 16px;
            margin-top: 32px;
            justify-content: flex-end;

            button {
                width: 164px;
            }
        }
    }
}


.updateFeedPostModal {
    h3 {
        margin: 0;
        margin-bottom: 32px;
    }
}

.updateMaterialModal {
    h3 {
        margin: 0;
        margin-bottom: 32px;
    }

    textarea{
        resize: none;
    }
}

.feedLabel {
    display: block;
    margin-bottom: 8px;

    font-weight: 600;
    font-size: 16px;

    color: var(--text-color-primary);
}


.feedTextarea {
    width: 100%;
    min-height: 120px;
    resize: vertical;
    resize: none;

    padding: 14px 16px;

    border-radius: 14px;
    border: 1px solid var(--border-color-secondary);
    box-shadow:  0 4px 30px rgba(0, 0, 0, 0.1);

    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);

    font-size: 15px;
    line-height: 1.5;
    color: var(--text-color-primary);

    transition:
        border-color 0.2s ease,
        box-shadow 0.2s ease,
        background-color 0.2s ease;

    &::placeholder {
        color: var(--text-color-tertiary);
    }

    &:hover {
        border-color: var(--accent-color-secondary-theme);
    }

    &:focus {
        outline: none;
        border-color: var(--accent-color-secondary-theme);
        background: rgba(255, 255, 255, 0.06);
    }

    &:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }
}

.editMode {
    .editable {
        @include app.editable;
        background: none;
        -webkit-text-fill-color: currentColor;
    }

    .categoryAndTags {
        padding: 4px;

        .tags {
            width: 100%;
            margin: 12px 0;

            >li {
                cursor: pointer;

                &::after {
                    content: '';
                    margin-left: 6px;
                    mask-image: url("../../../../public/icons/x.svg");
                    mask-size: cover;
                    display: inline-block;
                    width: 8px;
                    height: 8px;
                    background-color: var(--text-color-secondary);
                }
            }

            >div {
                display: flex;
                flex-direction: column;
                width: 100%;

                span {
                    p {
                        display: flex;
                        justify-content: space-between;
                        align-items: center;
                        width: 100%;
                    }
                }

                >* {
                    width: 100%;
                }

                input {
                    width: 100%;
                }
            }
        }
    }
}

.editControls {
    position: fixed;
    bottom: 5%;
    left: 50%;
    transform: translateX(-50%);
    display: grid;
    justify-items: center;
    filter: drop-shadow(0 16px 28px rgb(0 0 0 / 0.15));


    .bottom, .top {
        display: flex;
        gap: 16px;
        z-index: 1000;
        background-color: var(--background-color-secondary);
        border-radius: 16px;
        padding: 16px;
        width: max-content;

        a button {
            height: 100%;
            padding: 14px 24px;
        }

        .separator {
            width: 1px;
            background: var(--border-color-secondary);
            margin: 0 4px;
        }
    }

    .top {
        //width: 200px;
        padding-bottom: 24px;
        margin-bottom: -24px;
        justify-content: center;
    }
}


.modalButtons {
    display: flex;
    gap: 16px;
    margin-top: 32px;
    justify-content: flex-end;

    button {
        width: 164px;
    }
}


ul {
    list-style: none;
    padding: 0;
    margin: 0;
}

.createMaterialModal {
    h3 {
        margin: 0;
        margin-bottom: 32px;
    }
}

.createQuizModal {
    h3 {
        margin: 0;
        margin-bottom: 32px;
    }

    form {
        display: flex;
        flex-direction: column;
        gap: 16px;

        label {
            font-size: 16px;
            font-weight: 600;
        }

        input {
            width: 100%;
        }
    }
}

.course {
    display: flex;
    flex-direction: column;
    gap: 20px;

    .status {
        padding: 8px 18px;
        font-weight: 600;
        line-height: 1;
        border-radius: 10px;
        user-select: none;
        white-space: nowrap;
        width: fit-content;
        font-size: 20px;
        margin: 0;

        // Draft
        &[data-status="draft"] {
            color: var(--status-draft-text);
            background: var(--status-draft-bg);
        }

        // Scheduled
        &[data-status="scheduled"] {
            color: var(--status-scheduled-text);
            background: var(--status-scheduled-bg);
        }

        // Live
        &[data-status="live"] {
            color: var(--status-live-text);
            background: var(--status-live-bg);
        }

        // Paused
        &[data-status="paused"] {
            color: var(--status-paused-text);
            background: var(--status-paused-bg);
        }

        // Archived
        &[data-status="archived"] {
            color: var(--status-archived-text);
            background: var(--status-archived-bg);
        }
    }

    .statusSelect {
        border: none;
        cursor: pointer;
    }

    >.info {
        display: flex;
        justify-content: space-between;
        gap: 24px;
        align-items: start;

        >p {
            font-size: 18px;
            margin-bottom: 6px;
        }

        .brief {
            display: flex;
            flex-direction: column;
            min-width: 400px;
            gap: 24px;
            padding: 24px;
            border-radius: 16px;
            box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);

            >.fields {
                display: flex;

                >.el {
                    position: relative;
                    border-right: 1px solid color-mix(in srgb, var(--text-color-secondary) 30%, transparent 40%);
                    padding: 0 24px;
                    flex: 1;


                    >.title {
                        font-size: 16px;
                        margin-bottom: 8px;
                        width: max-content;
                    }

                    .nexp {
                        .item {
                            font-size: 24px;
                            margin: 0;
                        }
                    }

                    &:last-child {
                        border-right: none;
                        padding-right: 0;
                    }

                    &:first-child {
                        padding-left: 0;
                    }
                }
            }

            >.otherinfo {
                display: grid;
                gap: 16px;




            }

            .categoryAndTags {
                &:not(.editMode) {
                    overflow: hidden;
                }

                .wrp {
                    display: flex;
                    flex-direction: column;
                    gap: 12px;

                    .category {
                        display: flex;
                        align-items: center;
                        gap: 8px;
                        margin: 0;
                        font-size: 20px;
                        font-weight: 600;
                        color: var(--text-color-secondary);
                    }

                    .tags {
                        display: flex;
                        gap: 8px;
                        flex-wrap: wrap;

                        >li {
                            padding: 6px 12px;
                            border-radius: 12px;
                            background-color: var(--background-color-3);
                            margin: 0;
                            font-size: 14px;
                            font-weight: 500;
                            color: var(--text-color);
                            display: flex;
                            align-items: center;

                            p {
                                margin: 0;
                            }
                        }
                    }
                }
            }
        }
    }


    .modules {
        margin-top: 32px;

        nav {
            ul {
                display: flex;
                gap: 24px;

                li {
                    font-size: 18px;
                    padding-bottom: 8px;
                    cursor: pointer;
                    transition: all 0.2s;
                    user-select: none;
                    opacity: .6;
                    font-weight: 600;

                    &:hover {
                        color: var(--accent-color-secondary-theme);
                        opacity: 1;
                    }


                    &::after {
                        content: '';
                        position: absolute;
                        bottom: 0;
                        left: 0;
                        width: 100%;
                        height: 2px;
                        background-color: transparent;
                        border-radius: 4px;
                        transition: all 0.3s;
                    }

                    &.active {
                        border-bottom: none;
                        position: relative;
                        pointer-events: none;
                        opacity: 1;

                        &::after {
                            background-color: var(--accent-color-primary);
                        }
                    }
                }
            }
        }

        >div {
            padding: 24px;
            border-radius: 16px;
            box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);
            margin-top: 16px;
        }

        .materials {
            .addMaterialButton {
                margin-bottom: 16px;
                width: fit-content;
            }

            ul {
                display: flex;
                flex-direction: column;
                gap: 12px;
            }
        }

        .activity {

            .activityHeader{
                display: flex;
                //border-bottom: 1px solid color-mix(in srgb, var(--text-color-secondary) 20%, transparent 40%);
                //padding-bottom: 24px;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 16px;
                flex-wrap: wrap;
                gap: 16px;

                .feedPostFilter {


                    .feedFilterLabel {
                        margin: 0;
                        font-size: 18px;
                        font-weight: 700;
                        color: var(--text-color-secondary);
                    }

                    .feedFilterOptions {
                        display: flex;
                        align-items: center;
                        gap: 4px;
                        border-radius: 64px;


                        .feedFilterOption {
                            cursor: pointer;
                            user-select: none;
                            padding: 8px 16px;
                            border-radius: 64px;
                            transition: background-color 0.2s ease, color 0.2s ease;
                            border: 1px solid transparent;

                            &:is(.active) {
                                background-color: rgb(from var(--accent-color-primary) r g b / 0.25);
                                color: var(--accent-color-primary);
                                border-color: rgb(from var(--accent-color-primary) r g b / 0.6) rgb(from var(--accent-color-primary) r g b / 1.6) rgb(from var(--accent-color-primary) r g b / 0.6) rgb(from var(--accent-color-primary) r g b / 1.6);

                                // inset box shadow (liquid glass)
                                box-shadow: inset 0 0 10px rgb(from var(--accent-color-primary) r g b / 0.8);

                                &:hover{
                                    opacity: 0.8;
                                }
                            }

                            &:not(.active):hover {
                                background-color: var(--background-color-3);
                                filter: brightness(0.75);
                                transition-duration: 0.3s;
                            }

                            p {
                                margin: 0;
                                font-size: 16px;
                            }
                        }
                    }
                }
            }

            ul {
                display: flex;
                flex-direction: column;
                gap: 16px;

                li{
                    .feedDate {
                        display: flex;
                        flex-direction: column;
                        gap: 6px;
                        margin-bottom: 12px;

                        p {
                            margin: 0;
                            font-size: 16px;
                            font-weight: 500;
                            color: var(--accent-color-secondary-theme);
                        }
                    }

                    //.feedDateLine {
                    //    width: 90%;
                    //    height: 1px;
                    //    border-radius: 2px;
                    //    background: linear-gradient(90deg, var(--accent-color-primary) 0%, var(--accent-color-secondary) 100%);
                    //}

                    .feedPostWrapper {
                        display: flex;
                        position: relative;
                        border: 1px solid color-mix(in srgb, var(--text-color-secondary) 20%, transparent 40%);
                        border-left: none;
                        border-radius: 12px;
                        overflow: hidden;
                        min-width: 230px;

                        &:hover {
                            .feedPostActions {
                                opacity: 1;
                                pointer-events: auto;
                                transform: translateY(0);
                            }
                        }


                        .feedPostLeft{
                            display: flex;
                            justify-content: center;
                            padding: 16px;

                            .iconWrapper{
                                display: flex;
                                justify-content: center;
                                align-items: center;

                                border-radius: 48px;
                                height: 48px;
                                width: 48px;



                                .feedPostIcon{
                                    height: 24px;
                                    margin-left: 4px;
                                    aspect-ratio: 1/1;

                                    mask-size: cover;
                                    mask-position: center;
                                    mask-repeat: no-repeat;

                                    color: var(--background-color);
                                    background-color: var(--background-color);
                                }
                            }
                        }

                        .feedPostRight{
                            display: flex;
                            flex-direction: column;
                            padding: 16px 12px;
                            gap: 8px;

                            .feedPostHeader {
                                display: flex;
                                align-items: center;
                                gap: 8px;
                                flex-wrap: wrap;

                                .feedPurpose {
                                    display: flex;
                                    align-items: center;
                                    padding: 4px 8px;
                                    border-radius: 24px;

                                    p{
                                        height: auto;
                                        margin: 0;
                                    }
                                }

                                .feedTimestamp {
                                    font-size: 14px;
                                    color: var(--text-color-secondary);
                                    height: auto;
                                    margin: 0;
                                }
                            }

                            .feedPostAuthor {
                                display: flex;
                                align-items: center;
                                gap: 12px;

                                .feedAvatar {

                                }

                                .authorName {
                                    margin: 0;
                                    font-size: 18px;
                                    font-weight: 700;
                                }
                            }

                            .feedPostContent{

                                p{
                                    height: auto;
                                    margin: 0;
                                    margin-left: 3px;
                                    word-break: break-all;
                                }
                            }

                        }

                        .feedPostActions {
                            position: absolute;
                            top: 12px;
                            right: 12px;

                            display: flex;
                            gap: 8px;

                            opacity: 0;
                            pointer-events: none;
                            transform: translateY(-6px);

                            transition:
                                opacity 0.2s ease,
                                transform 0.2s ease;

                            // zmenšíme buttony pro feed
                            :global(button) {
                                padding: 6px 14px;
                                font-size: 14px;
                                background-color: var(--background-color-secondary);
                            }
                        }
                    }
                }
            }
        }
    }
}

.authModalHeader {
    margin-bottom: 24px;

    h3 {
        margin: 0;
        margin-bottom: 32px;
    }
}

.authTabs {
    display: flex;
    gap: 8px;
    border-bottom: 2px solid var(--background-color-3);
}

.authTab {
    flex: 1;
    padding: 12px 16px;
    background: transparent;
    border: none;
    border-bottom: 2px solid transparent;
    color: var(--text-color-secondary);
    font-size: 16px;
    font-weight: 600;
    border-radius: 24px 24px 0 0;
    cursor: pointer;
    transition: all 0.2s ease;
    margin-bottom: -2px;

    &:hover {
        color: var(--text-color-primary);
        background: var(--background-color-3);
    }

    &.active {
        color: var(--accent-color-primary);
        border-bottom-color: var(--accent-color-primary);
    }
}

.authFormContainer {
    margin-top: 24px;
}


// animace
.fadeEnterActive {
    transition: 300ms ease;
}

.fadeLeaveActive {
    transition: 200ms ease;
}

.fadeEnterFrom,
.fadeLeaveTo {
    opacity: 0;
}

.fadeEnterTo,
.fadeLeaveFrom {
    opacity: 1;
}

// laptop
@media screen and (max-width: app.$laptopBreakpoint) {
    .basic {
        flex-direction: column;

        .left {
            >p {
                text-align: justify;
            }
        }

        .right {
            width: 100%;
            max-width: none;
            margin-top: -32px;
            margin-bottom: 16px;

            .image {
                max-width: 380px;
                min-width: unset;
            }
        }
    }
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
    .basic {
        .right {}

        .left {
            .title {
                font-size: 64px;
            }
        }
    }

    .section {
        margin-top: -50px;
    }

    .title {
        font-size: clamp(40px, 8vw, 64px);
    }

    .course {
        >.info {
            flex-direction: column;
            align-items: stretch;

            >.brief {
                min-width: unset;
            }
        }
    }
}

@media screen and (max-width: 875px) {
    .editControls {
        flex-direction: column;
        align-items: stretch;
        gap: 8px;
        bottom: 2%;

        button {
            width: 100% !important;
            font-size: 12px !important;
        }
    }
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
    .basic {
        .right {}

        .left {
            .title {
                font-size: clamp(40px,8vw,64px);
            }
        }
    }

    .course {
        >.info {
            >.brief {
                .fields {
                    flex-direction: column;
                    gap: 8px;

                    >.el {
                        border-right: none !important;
                        padding: 0 !important;
                        display: flex;
                        align-items: center;
                        justify-content: space-between;

                        >p {
                            margin: 0 !important;
                            padding: 0;
                        }

                        &:last-child {
                            border-bottom: none !important;
                            padding-bottom: 0 !important;
                        }
                    }
                }

                .otherinfo {
                    .authorAndRating {
                        .rating {
                            width: 100%;
                        }
                    }
                }
            }
        }
    }

    .feedPostWrapper {
        .feedPostLeft {
            padding: 0 !important;

            .iconWrapper {
                display: none !important;
            }
        }
    }
}
</style>