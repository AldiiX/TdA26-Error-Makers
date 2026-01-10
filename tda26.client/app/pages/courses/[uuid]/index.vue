<script setup lang="ts">
import {type Account, type Course, type gRecaptcha, type Material, type Quiz} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import MaterialItem from "~/components/pagespecific/MaterialItem.vue";
import Modal from "~/components/Modal.vue";
import ModalDestructive from "~/components/ModalDestructive.vue";
import MaterialFormItem from "~/components/pagespecific/MaterialFormItem.vue";
import { ref, computed } from "vue";
import QuizItem from "~/components/pagespecific/QuizItem.vue";
import {ClientOnly, NuxtLink} from "#components";
import NumberExponential from "~/components/NumberExponential.vue";
import Avatar from "~/components/Avatar.vue";
import Input from "~/components/Input.vue";
import { push } from "notivue";
import SmoothSizeWrapper from "~/components/SmoothSizeWrapper.vue";
import LoginForm from "~/components/LoginForm.vue";
import RegisterForm from "~/components/RegisterForm.vue";

declare const grecaptcha: gRecaptcha;

definePageMeta({
    layout: "normal-page-layout",
    alias: ["/course/:uuid"],
    middleware: [
        defineNuxtRouteMiddleware(async (to) => {
            const uuid = to.params.uuid as string;

            // pokud chybi uuid
            if (!uuid) {
                return navigateTo("/courses");
            }

            // pokud je stranka /course/:uuid, perm presmeruje na /courses/:uuid
            if (to.path.startsWith("/course/")) {
                return navigateTo(`/courses/${uuid}`);
            }
        })
    ]
});

const loggedAccount = useState<Account | null>('loggedAccount');
const route = useRoute();
const uuid = route.params.uuid as string;
const isEditMode = route.query.edit === 'true';

const isActionInProgress = ref(false);

// server small fetch
const { data: _courseSmall, error: courseSmallError } = await useFetch<Course>(`${getBaseUrl()}/api/v2/courses/${uuid}`, {
    query: { full: false },
    server: true,
    key: `course-${uuid}-small`,
});

if (courseSmallError.value || !_courseSmall.value) {
    console.error("Error loading small course:", courseSmallError.value);
    await navigateTo('/courses');
}

const courseSmall = ref<Course>(_courseSmall.value!);

// Dynamic SEO based on course data
useSeo({
    title: courseSmall.value.name,
    description: courseSmall.value.description || `Zjistěte více o kurzu ${courseSmall.value.name} na Think Different Academy. Interaktivní vzdělávání s praxí.`,
    keywords: `kurz, ${courseSmall.value.name}, online vzdělávání, e-learning`,
    type: 'article'
});



// pokud je edit mode, musi byt prihlasen uzivatel a vlastnik kurzu
if (isEditMode) {
    if (loggedAccount.value?.type !== 'admin' && (!loggedAccount.value || loggedAccount.value.uuid !== courseSmall.value.account?.uuid)) {
        await navigateTo(`/courses/${uuid}`);
    }
}


// client full fetch
const { data: _course, pending: coursePending, error: courseError } = useFetch<Course>(() => getBaseUrl() + `/api/v2/courses/${uuid}`, {
    query: { full: true },
    server: false,
    key: `course-${uuid}-full`,
});

if (courseError.value) {
    console.error("Error loading full course:", courseError.value);
}

const course = ref<Course | null>(_course.value ?? null);
const originalCourse = ref<Course | null>(null);

watch(_course, (val) => {
    if (!val) return;

    course.value = structuredClone(val);
    originalCourse.value = structuredClone(val);
}, { immediate: true });

const isDirty = computed(() => {
    if (!course.value || !originalCourse.value) return false;

    return (
        course.value.name !== originalCourse.value.name ||
        course.value.description !== originalCourse.value.description
    );
});

const ratingLoading = ref<boolean>(false);
const menuItems = ['Materiály', "Kvízy", 'Aktivita'];
const selectedItem = ref(menuItems[0]);

const selectItem = (item: string) => {
    selectedItem.value = item;
};

const getHostname = (url?: string) => {
    try {
        return url ? new URL(url).hostname : ''
    } catch {
        return ''
    }
}

// frontendove poslani view eventu
onMounted(async () => {
    // Wait for reCAPTCHA to be ready before executing
    if (typeof grecaptcha === 'undefined') {
        console.warn('reCAPTCHA not loaded, skipping view event');
        return;
    }

    grecaptcha.ready(async () => {
        const captchaToken = await grecaptcha.execute(
            "6LfDQhksAAAAAEz_ujbJNian3-e-TfyKx8gzRaCL",
            { action: "submit" }
        );

        await fetch(`/api/v2/courses/${uuid}/view`, {
            method: 'POST',
            body: JSON.stringify({ token: captchaToken }),
            headers: {
                'Content-Type': 'application/json'
            }
        });
    });
})

const enabledModal = ref<"updateMaterial" | "deleteMaterial" | "createMaterial" | "deleteQuiz" | "createQuiz" | "loginRequired" | null>(null);
let selectedMaterial = ref<Material | null>(null);
let selectedQuiz = ref<Quiz | null>(null);
const authTab = ref<"login" | "register">("login");

const updateError = ref<string | null>(null);
const deleteError = ref<string | null>(null);

const editingMaterial = ref<any>(null);

const isThisCourseLiked = computed(() => {
    if (!loggedAccount.value || !courseSmall.value) return false;
    return loggedAccount.value.likes.some(l => l.course?.uuid === courseSmall.value!.uuid);
});

const isThisCourseDisliked = computed(() => {
    if (!loggedAccount.value || !courseSmall.value) return false;
    return loggedAccount.value.dislikes.some(l => l.course?.uuid === courseSmall.value!.uuid);
});

const isThisCourseLikedDesign = ref<boolean>(isThisCourseLiked.value);
const isThisCourseDislikedDesign = ref<boolean>(isThisCourseDisliked.value);
const optimisticLikeCount = ref<number>(courseSmall.value?.likeCount ?? 0);

// Keep optimistic count in sync with fetched data
watch(() => courseSmall.value?.likeCount, (newCount) => {
    if (newCount !== undefined) {
        optimisticLikeCount.value = newCount;
    }
}, { immediate: true });



const openCreateMaterialModal = () => {
    editingMaterial.value = {
        name: "",
        description: "",
        type: "file",
        file: null,
        url: ""
    };
    enabledModal.value = "createMaterial";
};

const openUpdateMaterialModal = (material: Material) => {
    selectedMaterial.value = material;
    editingMaterial.value = JSON.parse(JSON.stringify(material)); // deep clone
    enabledModal.value = "updateMaterial";
};

const openDeleteMaterialModal = (material: Material) => {
    selectedMaterial.value = material;
    enabledModal.value = 'deleteMaterial';
};

const handleMaterialUpdate = async () => {
    if (!course.value || !selectedMaterial.value || !editingMaterial.value) return;

    const material = selectedMaterial.value;
    const edited = editingMaterial.value;
    
    isActionInProgress.value = true;

    const url = getBaseUrl() + `/api/v2/courses/${course.value.uuid}/materials/${material.uuid}`;

    if (edited.name.trim().length === 0) {
        updateError.value = "Název materiálu je povinný.";
        return;
    }

    try {
        let updatedMaterial;

        if (material.type === 'url') {
            try {
                edited.url = formatUrl(edited.url);
            } catch (error) {
                updateError.value = "Zadaná URL adresa není platná.";
                return;
            }
            
            // JSON update
            updatedMaterial = await $fetch<Material>(url, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: {
                    name: edited.name,
                    description: edited.description,
                    url: edited.url,
                }
            });
        } else {
            // FILE MATERIAL UPDATE (multipart/form-data)
            const form = new FormData();
            form.append("Name", edited.name ?? "");
            form.append("Description", edited.description ?? "");

            // if a NEW FILE was selected
            if (edited.file instanceof File) {
                form.append("File", edited.file);
            }

            updatedMaterial = await $fetch<Material>(url, {
                method: "PUT",
                body: form
            });
        }

        // update local state
        course.value.materials = course.value.materials!.map(m =>
            m.uuid === updatedMaterial.uuid ? updatedMaterial : m
        );

        enabledModal.value = null;

    } catch (err) {
        console.error("Update failed:", err);
    } finally {
        isActionInProgress.value = false;
    }
};

const handleMaterialDelete = async () => {
    if (!course.value || !course.value.materials) return;
    
    isActionInProgress.value = true;

    await $fetch<void>(getBaseUrl() + `/api/v2/courses/${course.value.uuid}/materials/${selectedMaterial.value?.uuid}`, {
        method: 'DELETE'
    }).then(() => {
        // remove from local state
        course.value!.materials = course.value!.materials!.filter(m => m.uuid !== selectedMaterial.value?.uuid);
        enabledModal.value = null;
        deleteError.value = null;
    }).catch((err) => {
        console.error("Error deleting material:", err);
        deleteError.value = "Nepodařilo se smazat materiál. Zkuste to prosím znovu.";
    }).finally(() => {
        isActionInProgress.value = false;
    });
};

const handleMaterialCreate = async () => {
    if (!course.value || !editingMaterial.value) return;

    const edited = editingMaterial.value;
    
    isActionInProgress.value = true;

    const url = getBaseUrl() + `/api/v2/courses/${course.value.uuid}/materials`;

    if (edited.name.trim().length === 0) {
        updateError.value = "Název materiálu je povinný.";
        return;
    }

    try {
        let createdMaterial;

        if (edited.type === "url") {
            try {
                edited.url = formatUrl(edited.url);
            } catch (error) {
                updateError.value = "Zadaná URL adresa není platná.";
                return;
            }
            
            // JSON create
            createdMaterial = await $fetch<Material>(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: {
                    type: "url",
                    name: edited.name,
                    description: edited.description,
                    url: edited.url,
                }
            });
        } else {
            // FILE MATERIAL CREATE (multipart/form-data)
            const form = new FormData();
            form.append("Type", "file");
            form.append("Name", edited.name ?? "");
            form.append("Description", edited.description ?? "");
            form.append("File", edited.file);

            if (edited.file instanceof File) {
                form.append("File", edited.file);
            } else {
                throw new Error("No file selected for file material.");
            }

            createdMaterial = await $fetch<Material>(url, {
                method: "POST",
                body: form
            });
        }

        // add to local state
        course.value.materials = course.value.materials ?? [];
        course.value.materials.unshift(createdMaterial);

        enabledModal.value = null;

    } catch (err) {
        console.error("Creation failed:", err);
    } finally {
        isActionInProgress.value = false;
    }
};

function formatUrl(url: string): string {
    if (!url.startsWith('http://') && !url.startsWith('https://')) {
        url = 'https://' + url;
    }
    
    const urlRegex = new RegExp(/[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/gi);
    
    if (!urlRegex.test(url)) {
        throw new Error("Invalid URL format");
    }
    
    const parsedUrl = new URL(url);
    
    return parsedUrl.href;
}

const ownsCourse = computed(() => {
    if (!loggedAccount.value || !courseSmall.value) return false;
    if(loggedAccount.value.type === 'admin') return true;
    return loggedAccount.value?.uuid === courseSmall.value?.account?.uuid;
});

async function addRating(rating: "like" | "dislike" | null) {
    // Check if user is logged in, if not show login modal
    if (!loggedAccount.value) {
        enabledModal.value = "loginRequired";
        return;
    }
    
    if (!courseSmall.value || ratingLoading.value) return;

    const baseUrl = getBaseUrl();
    const uuid = courseSmall.value.uuid;
    const url = baseUrl + `/api/v2/courses/${uuid}/rating`;

    // Store previous state for rollback
    const previousLikedDesign = isThisCourseLikedDesign.value;
    const previousDislikedDesign = isThisCourseDislikedDesign.value;
    const previousLikeCount = optimisticLikeCount.value;

    switch (rating) {
        case "like": {
            if (isThisCourseLikedDesign.value) {
                // Unliking: remove the like
                isThisCourseLikedDesign.value = false;
                optimisticLikeCount.value--;
                rating = null;
            } else {
                // Liking (either from no rating or from dislike)
                isThisCourseLikedDesign.value = true;
                isThisCourseDislikedDesign.value = false;
                optimisticLikeCount.value++;
            }
        } break;

        case "dislike": {
            if (isThisCourseDislikedDesign.value) {
                // Removing dislike: no change to like count
                isThisCourseDislikedDesign.value = false;
                rating = null;
            } else {
                // Disliking (either from no rating or from like)
                // If transitioning from like to dislike, remove the like
                if (isThisCourseLikedDesign.value) {
                    optimisticLikeCount.value--;
                }
                isThisCourseDislikedDesign.value = true;
                isThisCourseLikedDesign.value = false;
            }
        } break;
    }

    try {
        ratingLoading.value = true;

        await $fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: { type: rating }
        });

        // spusteni vsech refreshu paralelne
        const userPromise = $fetch<Account>(baseUrl + `/api/v2/me`, {
            method: "GET",
            headers: { "Content-Type": "application/json" },
        });

        const courseSmallPromise = $fetch<Course>(baseUrl + `/api/v2/courses/${uuid}`, {
            method: "GET",
            headers: { "Content-Type": "application/json" },
            query: { full: false },
        });

        const coursePromise = $fetch<Course>(baseUrl + `/api/v2/courses/${uuid}`, {
            method: "GET",
            headers: { "Content-Type": "application/json" },
            query: { full: true },
        });

        const [updatedUser, updatedCourseSmall, updatedCourse] = await Promise.all([
            userPromise,
            courseSmallPromise,
            coursePromise,
        ]);

        // hromadne prirazeni dat az po dokonceni vsech requestu
        loggedAccount.value = updatedUser ?? null;
        courseSmall.value = updatedCourseSmall;
        course.value = updatedCourse;
        // optimisticLikeCount is automatically synced via watcher
    }

    catch(err) {
        console.error("Error updating rating:", err);
        // Rollback optimistic updates on error
        isThisCourseLikedDesign.value = previousLikedDesign;
        isThisCourseDislikedDesign.value = previousDislikedDesign;
        optimisticLikeCount.value = previousLikeCount;
    }

    finally {
        ratingLoading.value = false;
    }
}

const handleQuizDelete = async () => {
    if (!course.value || !course.value.quizzes) return;
    
    isActionInProgress.value = true;

    await $fetch<void>(getBaseUrl() + `/api/v1/courses/${course.value.uuid}/quizzes/${selectedQuiz.value?.uuid}`, {
        method: 'DELETE'
    }).then(() => {
        course.value!.quizzes = course.value!.quizzes!.filter(q => q.uuid !== selectedQuiz.value?.uuid);
        push.success({
            title: "Kvíz smazán",
            message: "Kvíz byl úspěšně smazán.",
            duration: 4000
        });
        enabledModal.value = null;
        deleteError.value = null;
    }).catch((err) => {
        console.error("Error deleting quiz:", err);
        deleteError.value = "Nepodařilo se smazat kvíz. Zkuste to prosím znovu.";
    }).finally(() => {
        isActionInProgress.value = false;
    });
};

const handleQuizCreate = async (e: Event) => {
    if (!course.value) return;
    
    isActionInProgress.value = true;

    const form = e.target as HTMLFormElement;
    const formData = new FormData(form);
    const quizName = formData.get('createQuizName')?.toString().trim();

    if (!quizName) {
        updateError.value = "Název kvízu je povinný.";
        return;
    }

    try {
        const newQuiz = await $fetch<Quiz>(
            `${getBaseUrl()}/api/v1/courses/${course.value.uuid}/quizzes`,
            {
                method: 'POST',
                body: { title: quizName }
            }
        );

        course.value.quizzes = course.value.quizzes ?? [];
        newQuiz.createdAt = new Date().toISOString();
        course.value.quizzes.unshift(newQuiz);

        push.success({
            title: "Kvíz vytvořen",
            message: "Kvíz byl úspěšně vytvořen.",
            duration: 4000
        });

        enabledModal.value = null;
        updateError.value = null;
        form.reset();
    } catch (err) {
        console.error("Creation failed:", err);
        updateError.value = "Nepodařilo se vytvořit kvíz. Zkuste to prosím znovu.";
    } finally {
        isActionInProgress.value = false;
    }
};

const updateCourseTitle = (newTitle: string) => {
    if (!isEditMode) return;
    
    if (course.value) {
        course.value.name = newTitle;
    }
};

const updateCourseDescription = (newDescription: string) => {
    if (!isEditMode) return;
    
    if (course.value) {
        course.value.description = newDescription;
    }
};

const saveCourseChanges = async () => {
    if (!course.value) return;
    
    isActionInProgress.value = true;

    // validace
    if (course.value.name.trim().length === 0) {
        push.error({
            title: "Chyba při ukládání",
            message: "Název kurzu nesmí být prázdný.",
            duration: 4000
        });
        
        isActionInProgress.value = false;
        
        return;
    }

    if(course.value.name.length > 128) {
        push.error({
            title: "Chyba při ukládání",
            message: "Název kurzu nesmí být delší než 128 znaků.",
            duration: 4000
        });
        
        isActionInProgress.value = false;
        return;
    }

    try {
        const updatedCourse = await $fetch<Course>(getBaseUrl() + `/api/v2/courses/${course.value.uuid}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: {
                name: course.value.name,
                description: course.value.description,
            }
        });

        originalCourse.value = structuredClone(updatedCourse);
        push.success({
            title: "Změny uloženy",
            message: "Změny kurzu byly úspěšně uloženy.",
            duration: 4000
        })
    } catch (err) {
        console.error("Error saving course changes:", err);
        push.error({
            title: "Chyba při ukládání",
            message: "Nepodařilo se uložit změny kurzu. Zkuste to prosím znovu.",
            duration: 4000
        });
    } finally {
        isActionInProgress.value = false;
    }
};

const editBackClick = () => {
    window.location.href = `/courses/${courseSmall.value?.uuid}`;
};

const editClick = () => {
    window.location.href = `/courses/${courseSmall.value?.uuid}?edit=true`;
};

const handleAuthSuccess = (account: Account) => {
    // Close the modal
    enabledModal.value = null;
    
    // Reload the page to refresh course data with new user context
    window.location.reload();
};

onMounted(() => {
    // Warn user about unsaved changes
    if (!import.meta.dev) {
        window.addEventListener("beforeunload", (e) => {
            if (!isDirty.value) return;
            
            e.preventDefault();
            e.returnValue = "";
        });
    }
});

</script>

<template>
    <Head>
        <Title>{{ courseSmall?.name ?? "Kurz" }} • Think different Academy</Title>
    </Head>

    <div :class="[$style.course, isEditMode && $style.editMode]">
        <h1 
            :class="['text-gradient', $style.title, $style.editable]"
            :contenteditable="isEditMode"
            @input="(e) => updateCourseTitle((e.target as HTMLElement).innerText.trim())"
        >{{ courseSmall?.name }}</h1>
        <div :class="$style.info">
            <p
                :class="[$style.editable]"
                :contenteditable="isEditMode"
                @input="(e) => updateCourseDescription((e.target as HTMLElement).innerText.trim())"
            >{{ courseSmall?.description }}</p>
            <div :class="['liquid-glass', $style.brief]">
                <div :class="$style.fields">
                    <div :class="$style.el">
                        <p :class="$style.title">Zhlédnutí</p>
                        <NumberExponential :value="courseSmall?.viewCount ?? 0" :container-class="$style.nexp" :numberClass="$style.item" />
                    </div>
                    <div :class="$style.el">
                        <p :class="$style.title">Materiály</p>
                        <NumberExponential :value="course?.materials?.length ?? 0" :container-class="$style.nexp" :numberClass="$style.item" />
                    </div>
                    <div :class="$style.el">
                        <p :class="$style.title">Kvízy</p>
                        <NumberExponential :value="course?.quizzes?.length ?? 0" :container-class="$style.nexp" :numberClass="$style.item" /> <!-- TODO: dodělat recenze -->
                    </div>
                </div>

                <div :class="$style.otherinfo">
                    <div :class="$style.authorAndRating">
                        <NuxtLink v-if="courseSmall?.account" :class="[$style.author, { [$style.clickable]: courseSmall.lecturer }]" :to="courseSmall?.lecturer ? `/lecturer/${courseSmall?.lecturer?.uuid}` : '' ">
                            <Avatar :class="$style.avatar" :letter-style="{ color: 'var(--accent-color-secondary-theme-text)' }" :name="courseSmall?.lecturer?.fullName ?? courseSmall?.account?.fullName ?? '?'" :src="courseSmall?.lecturer?.pictureUrl ?? null" />
                            <p>{{ courseSmall?.lecturer?.fullName ?? courseSmall?.account?.fullName }}</p>
                        </NuxtLink>

                        <div :class="$style.rating">
                            <!-- like a dislike button -->
                            <div :class="[$style.duo, { [$style.active]: isThisCourseLikedDesign  }]" @click="addRating('like')">
                                <div :class="$style.icon"></div>
                                <p>{{ optimisticLikeCount }}</p>
                            </div>

                            <div :class="[$style.duo, { [$style.active]: isThisCourseDislikedDesign }]" @click="addRating('dislike')">
                                <div :class="$style.icon" style="rotate: 180deg"></div>
                                <p>Nelíbí se</p>
                            </div>
                        </div>
                    </div>
                    <Button 
                        v-if="ownsCourse && !isEditMode"
                        button-style="primary"
                        accent-color="secondary"
                        @click="editClick"
                    >Upravit kurz</Button>
                </div>
            </div>
        </div>

        <div :class="$style.details">
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
            <div :class="['liquid-glass']" style="overflow: hidden">
                <SmoothSizeWrapper>
                    <ClientOnly>
                        <div v-if="selectedItem == 'Materiály'" :class="$style.materials">
                            <Button v-if="ownsCourse" button-style="primary" accent-color="primary" @click="openCreateMaterialModal" :class="$style.addMaterialButton">
                                Přidat nový materiál
                            </Button>

                            <p v-if="coursePending">Načítání materiálů...</p>
                            <p v-else-if="course?.materials === undefined || course.materials.length == 0">Tento kurz nemá žádné materiály.</p>
                            <ul v-else>
                                <li v-for="material in course.materials" :key="material.uuid">
                                    <MaterialItem
                                        :material="material"
                                        :course="course"
                                        :edit-mode="ownsCourse"
                                        @edit="openUpdateMaterialModal"
                                        @delete="openDeleteMaterialModal"
                                    />
                                </li>
                            </ul>
                        </div>
                        <div v-if="selectedItem == 'Kvízy'" :class="$style.materials">
                            <Button v-if="ownsCourse" button-style="primary" accent-color="primary" @click="enabledModal = 'createQuiz'" :class="$style.addMaterialButton">
                                Přidat nový kvíz
                            </Button>

                            <p v-if="coursePending">Načítání kvízů...</p>
                            <p v-else-if="course?.quizzes === undefined || course.quizzes.length == 0">Tento kurz nemá žádné kvízy.</p>
                            <ul v-else>
                                <li v-for="quiz in course.quizzes" :key="quiz.uuid">
                                    <QuizItem
                                        :quiz="quiz"
                                        :course="course"
                                        :edit-mode="ownsCourse"
                                        @delete="(q) => { selectedQuiz = q; enabledModal = 'deleteQuiz'; }"
                                    />
                                </li>
                            </ul>
                        </div>
                        <div v-if="selectedItem == 'Aktivita'" :class="$style.activity">
                            <!--                    <p v-if="course.feed.length == 0">Žádná nedávná aktivita.</p>-->
                            <!--                    <ul v-else>-->
                            <!--                        <li v-for="feedPost in course.feed" :key="feedPost.uuid">-->
                            <!--                            &lt;!&ndash; // TODO: &ndash;&gt;-->
                            <!--                        </li>-->
                            <!--                    </ul>-->
                        </div>
                    </ClientOnly>
                </SmoothSizeWrapper>
            </div>
        </div>
    </div>


    <Teleport to="#teleports">
        <!-- Edit controls -->
        <div
            v-if="isEditMode"
            :class="$style.editControls"
        >
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
                @click="saveCourseChanges(); editBackClick()"
            >
                Uložit a ukončit úpravy
            </Button>
            <Button
                buttonStyle="tertiary"
                @click="editBackClick"
                :disabled="isActionInProgress"
                >
                Ukončit úpravy
            </Button>
        </div>
        
        
        
        
        <!-- CREATE -->
        <Modal
            :enabled="enabledModal === 'createMaterial'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
            :class="$style.createMaterialModal"
        >
            <h3>Vytvoření nového materiálu</h3>
            <MaterialFormItem
                v-model="editingMaterial"
                :index="0"
                :showRemoveButton="false"
                @file-selected="(_, file) => editingMaterial.file = file"
            />
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" @click="enabledModal = null" :disabled="isActionInProgress">Zrušit</Button>

                <Button
                    button-style="primary"
                    accent-color="secondary"
                    @click="handleMaterialCreate"
                    :disabled="isActionInProgress"
                >
                    Vytvořit materiál
                </Button>
            </div>
            <p v-if="updateError" class="error-text">{{ updateError }}</p>
        </Modal>

        <!-- UPDATE -->
        <Modal
            :enabled="enabledModal === 'updateMaterial'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
        >
            <h3>Úprava materiálu</h3>

            <MaterialFormItem
                v-if="editingMaterial"
                v-model="editingMaterial"
                :index="0"
                :showRemoveButton="false"
                @file-selected="(_, file) => editingMaterial.file = file"
            />


            <div :class="$style.modalButtons">
                <Button button-style="tertiary" @click="enabledModal = null" :disabled="isActionInProgress">Zrušit</Button>

                <Button
                    button-style="primary"
                    accent-color="secondary"
                    @click="handleMaterialUpdate"
                    :disabled="isActionInProgress"
                >
                    Uložit změny
                </Button>
            </div>

            <p v-if="updateError" class="error-text">{{ updateError }}</p>
        </Modal>

        <!-- DELETE MATERIAL -->
        <ModalDestructive
            :enabled="enabledModal === 'deleteMaterial'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            title="Potvrzení akce"
            description="Opravdu chceš smazat tento materiál? Tuto akci nelze vrátit zpět."
            :yes-action="handleMaterialDelete"
        >
            <h3>Opravdu si přeješ smazat materiál <i class="text-gradient">{{ selectedMaterial?.name }}</i>?</h3>
            <p>Tuto akci nelze vrátit zpět.</p>
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" @click="enabledModal = null" :disabled="isActionInProgress">Zrušit</Button>
                <Button
                    button-style="primary"
                    accent-color="secondary"
                    @click="handleMaterialDelete"
                    :disabled="isActionInProgress"
                >
                    Smazat materiál
                </Button>
            </div>
            <p v-if="deleteError" class="error-text">{{ deleteError }}</p>
        </ModalDestructive>

        <!-- DELETE QUIZ -->
        <ModalDestructive
            :enabled="enabledModal === 'deleteQuiz'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            title="Potvrzení akce"
            description="Opravdu chceš smazat tento kvíz? Tuto akci nelze vrátit zpět."
            :yesAction="handleQuizDelete"
        >
            <h3>Opravdu si přeješ smazat kvíz <i class="text-gradient">{{ selectedQuiz?.title }}</i>?</h3>
            <p>Tuto akci nelze vrátit zpět.</p>
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" @click="enabledModal = null" :disabled="isActionInProgress">Zrušit</Button>
                <Button
                    button-style="primary"
                    accent-color="secondary"
                    @click="handleQuizDelete"
                    :disabled="isActionInProgress"
                >
                    Smazat kvíz
                </Button>
            </div>
            <p v-if="deleteError" class="error-text">{{ deleteError }}</p>
        </ModalDestructive>
        
        <!-- CREATE QUIZ -->
        <Modal
            :enabled="enabledModal === 'createQuiz'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
            :class-name="$style.createQuizModal"
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
                    <Button button-style="tertiary" @click="enabledModal = null" :disabled="isActionInProgress">Zrušit</Button>

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
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '500px' }"
        >
            <SmoothSizeWrapper>
                <div :class="$style.authModalHeader">
                    <h3>Pro hodnocení kurzu se musíš přihlásit.</h3>

                    <div :class="$style.authTabs">
                        <button
                            :class="[$style.authTab, { [$style.active]: authTab === 'login' }]"
                            @click="authTab = 'login'"
                            type="button"
                        >
                            Přihlášení
                        </button>

                        <button
                            :class="[$style.authTab, { [$style.active]: authTab === 'register' }]"
                            @click="authTab = 'register'"
                            type="button"
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
    </Teleport>
</template>

<style module lang="scss">
@use "../../../app" as *;

.editMode {
    .editable {
        @include editable;
        background: none;
        -webkit-text-fill-color: currentColor;
    }
}

.editControls {
    position: fixed;
    bottom: 5%;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    gap: 16px;
    z-index: 1000;
    background-color: var(--background-color-secondary);
    border-radius: 16px;
    padding: 16px;
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    
    a button {
        height: 100%;
        padding: 14px 24px;
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

.title{
    font-size: 72px;
    margin: 0;
    overflow: visible;
    width: fit-content;
    padding-bottom: 4px;
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

                .authorAndRating {
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
            }
        }
    }


    .details {
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

            }

            ul {
                display: flex;
                flex-direction: column;
                gap: 12px;
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
</style>