<script setup lang="ts">
    import {type Account, type Course, type gRecaptcha, type Material} from "#shared/types";
    import getBaseUrl from "#shared/utils/getBaseUrl";
    import Button from "~/components/Button.vue";
    import MaterialItem from "~/components/pagespecific/MaterialItem.vue";
    import Modal from "~/components/Modal.vue";
    import MaterialFormItem from "~/components/pagespecific/MaterialFormItem.vue";
    import {computed, ref} from "vue";
    import {ClientOnly, Head, NuxtLink, Title} from "#components";
    import NumberExponential from "~/components/NumberExponential.vue";
    import Avatar from "~/components/Avatar.vue";

    declare const grecaptcha: gRecaptcha;

    definePageMeta({
        layout: "normal-page-layout",
        alias: ["/courses/:uuid"],
        middleware: [
            defineNuxtRouteMiddleware(async (to) => {                
                const uuid = to.params.uuid as string;
                
                // pokud chybi uuid
                if (!uuid) {
                    return navigateTo("/courses");
                }

                // pokud je stranka /courses/:uuid, perm presmeruje na /course/:uuid
                if (to.path.startsWith("/courses/")) {
                    return navigateTo(`/course/${uuid}`);
                }
            })
        ]
    });

    const loggedUser = useState<Account | null>('loggedAccount');
    const route = useRoute();
    const uuid = route.params.uuid as string;

    // server small fetch
    const { data: courseSmall, error: courseSmallError } = await useFetch<Course>(`${getBaseUrl()}/api/v2/courses/${uuid}`, {
        query: { full: false },
        server: true,
        key: `course-${uuid}-small`,
    });

    if (courseSmallError.value || !courseSmall.value) {
        console.error("Error loading small course:", courseSmallError.value);
        await navigateTo('/courses');
    }

    // client full fetch
    const { data: course, pending: coursePending, error: courseError } = useFetch<Course>(() => getBaseUrl() + `/api/v2/courses/${uuid}`, {
        query: { full: true },
        server: false,
        key: `course-${uuid}-full`,
    });

    if (courseError.value) {
        console.error("Error loading full course:", courseError.value);
    }
    
    const ratingLoading = ref<boolean>(false);
    const menuItems = ['Materiály', 'Aktivita'];
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
    })
    
    const enabledModal = ref<"updateMaterial" | "deleteMaterial" | "createMaterial" | null>(null);
    let selectedMaterial = ref<Material | null>(null);
    
    const updateError = ref<string | null>(null);
    const deleteError = ref<string | null>(null);

    const editingMaterial = ref<any>(null);

    const isThisCourseLiked = computed(() => {
        if (!loggedUser.value || !courseSmall.value) return false;
        return loggedUser.value.likes.some(l => l.course?.uuid === courseSmall.value!.uuid);
    });

    const isThisCourseDisliked = computed(() => {
        if (!loggedUser.value || !courseSmall.value) return false;
        return loggedUser.value.dislikes.some(l => l.course?.uuid === courseSmall.value!.uuid);
    });

    const isThisCourseLikedDesign = ref<boolean>(isThisCourseLiked.value);
    const isThisCourseDislikedDesign = ref<boolean>(isThisCourseDisliked.value);
    const optimisticLikeCount = ref<number>(courseSmall.value?.likeCount ?? 0);



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
    
        const url = getBaseUrl() + `/api/v2/courses/${course.value.uuid}/materials/${material.uuid}`;
    
        try {
            let updatedMaterial;
    
            if (material.type === "url") {
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

                console.log("Submitting form data:", {
                    Name: edited.name,
                    Description: edited.description,
                    File: edited.file instanceof File ? edited.file.name : "No new file"
                });
    
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
        }
    };
    
    const handleMaterialDelete = async () => {
        if (!course.value || !course.value.materials) return;
        
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
        });
    };
    
    const handleMaterialCreate = async () => {
        if (!course.value || !editingMaterial.value) return;
        
        const edited = editingMaterial.value;
        
        const url = getBaseUrl() + `/api/v2/courses/${course.value.uuid}/materials`;
        
        try {
            let createdMaterial;
            
            if (edited.type === "url") {
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
        }
    };

async function addRating(rating: "like" | "dislike" | null) {
    if (!loggedUser.value || !courseSmall.value || ratingLoading.value) return;

    const baseUrl = getBaseUrl();
    const uuid = courseSmall.value.uuid;
    const url = baseUrl + `/api/v2/courses/${uuid}/rating`;

    switch (rating) {
        case "like": {
            if (isThisCourseLiked.value) {
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
            if (isThisCourseDisliked.value) {
                // Removing dislike: no change to like count
                isThisCourseDislikedDesign.value = false;
                rating = null;
            } else {
                // Disliking (either from no rating or from like)
                isThisCourseDislikedDesign.value = true;
                isThisCourseLikedDesign.value = false;
                // If transitioning from like to dislike, remove the like
                if (isThisCourseLiked.value) {
                    optimisticLikeCount.value--;
                }
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
        loggedUser.value = updatedUser ?? null;
        courseSmall.value = updatedCourseSmall;
        course.value = updatedCourse;
        optimisticLikeCount.value = updatedCourseSmall.likeCount ?? 0;
    }

    catch(err) {
        console.error("Error updating rating:", err);
    }

    finally {
        ratingLoading.value = false;
    }
}
</script>

<template>
    <Head>
        <Title>{{ courseSmall?.name ?? "Kurz" }} • Think different Academy</Title>
    </Head>

    <div :class="$style.course">
        <h1 :class="['text-gradient', $style.title]">{{ courseSmall?.name }}</h1>
        <div :class="$style.info">
            <p>{{ courseSmall?.description }}</p>
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
                        <p :class="$style.title">Recenze</p>
                        <NumberExponential :value="optimisticLikeCount" :container-class="$style.nexp" :numberClass="$style.item" />
                    </div>
                </div>

                <div :class="$style.otherinfo">
                    <div :class="$style.authorAndRating">
                        <NuxtLink v-if="courseSmall?.lecturer" :class="$style.author" :to="`/lecturer/${courseSmall?.lecturer?.uuid}`">
                            <Avatar :class="$style.avatar" :name="courseSmall?.lecturer?.fullName ?? '?'" :src="courseSmall?.lecturer?.pictureUrl ?? null" />
                            <p>{{ courseSmall?.lecturer?.fullName }}</p>
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
            <div :class="['liquid-glass']">
                <ClientOnly>
                    <div v-if="selectedItem == 'Materiály'" :class="$style.materials">
                        <Button v-if="loggedUser?.uuid == course?.lecturer?.uuid" button-style="primary" accent-color="secondary" @click="openCreateMaterialModal" :class="$style.addMaterialButton">
                            Přidat nový materiál
                        </Button>

                        <p v-if="coursePending">Načítání materiálů...</p>
                        <p v-else-if="course?.materials === undefined || course.materials.length == 0">Tento kurz nemá žádné materiály.</p>
                        <ul v-else>
                            <li v-for="material in course.materials" :key="material.uuid">
                                <MaterialItem
                                    :material="material"
                                    :course="course"
                                    :edit-mode="loggedUser?.uuid == courseSmall?.lecturer?.uuid"
                                    @edit="openUpdateMaterialModal"
                                    @delete="openDeleteMaterialModal"
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
            </div>
        </div>
    </div>


    <Teleport to="#teleports">
        <!-- CREATE -->
        <Modal
            :enabled="enabledModal === 'createMaterial'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
        >
            <h3>Vytvoření nového materiálu</h3>
            <MaterialFormItem
                v-model="editingMaterial"
                :index="0"
                :showRemoveButton="false"
                @file-selected="(_, file) => editingMaterial.file = file"
            />
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" @click="enabledModal = null">Zrušit</Button>

                <Button
                    button-style="primary"
                    accent-color="secondary"
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
                <Button button-style="tertiary" @click="enabledModal = null">Zrušit</Button>

                <Button
                    button-style="primary"
                    accent-color="secondary"
                    @click="handleMaterialUpdate"
                >
                    Uložit změny
                </Button>
            </div>

            <p v-if="updateError" class="error-text">{{ updateError }}</p>
        </Modal>

        <!-- DELETE -->
        <Modal
            :enabled="enabledModal === 'deleteMaterial'"
            @close="enabledModal = null"
            can-be-closed-by-clicking-outside
            :modalStyle="{ maxWidth: '800px' }"
        >
            <h3>Opravdu si přeješ smazat materiál <i class="text-gradient">{{ selectedMaterial?.name }}</i>?</h3>
            <p>Tuto akci nelze vrátit zpět.</p>
            <div :class="$style.modalButtons">
                <Button button-style="tertiary" @click="enabledModal = null">Zrušit</Button>
                <Button
                    button-style="primary"
                    accent-color="secondary"
                    @click="handleMaterialDelete"
                >
                    Smazat materiál
                </Button>
            </div>
            <p v-if="deleteError" class="error-text">{{ deleteError }}</p>
        </Modal>
    </Teleport>
</template>

<style module lang="scss">
.modalButtons {
    display: flex;
    gap: 16px;
    margin-top: 24px;
}

.title{
    font-size: 72px;
    margin: 0;
    overflow: visible;
    width: fit-content;
}

ul {
    list-style: none;
    padding: 0;
    margin: 0;
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

                        &:hover {
                            opacity: 0.5;
                            transition-duration: 0.3s;
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
</style>