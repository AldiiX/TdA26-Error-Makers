<script setup lang="ts">
import {type Account, type Course, type Material} from "#shared/types";
    import getBaseUrl from "#shared/utils/getBaseUrl";
    import Button from "~/components/Button.vue";
import MaterialItem from "~/components/pagespecific/MaterialItem.vue";
import CourseForm from "~/components/pagespecific/CourseForm.vue";
import Modal from "~/components/Modal.vue";
import MaterialFormItem from "~/components/pagespecific/MaterialFormItem.vue";

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

    const route = useRoute();
    const uuid = route.params.uuid as string;

    // server small fetch
    const { data: _courseSmall, error: errorCourseSmall } = await useFetch<Course>(`/api/v2/courses/${uuid}?full=false`);
    
    if (errorCourseSmall.value) {
        console.error("Error loading course:", errorCourseSmall.value);
        await navigateTo('/dashboard/courses');
    }
    
    const courseSmall = computed(() => _courseSmall.value ?? null);
    
    if (!courseSmall.value) {
        console.error("Course small fetch returned null.");
        await navigateTo('/dashboard/courses');
    }
    
    
    // client full fetch
    const { data: _course, pending: coursePending, error: courseError } = await useFetch<Course>(getBaseUrl() + `/api/v2/courses/${uuid}?full=true`, {
        server: false,
    });
    
    if (courseError.value) {
        console.error("Error loading full course:", courseError.value);
    }
    
    const course = computed(() => _course.value)

    const user = useState<Account | null>('loggedAccount');

    const menuItems = ['Materiály', 'Aktivita'];
    const selectedItem = ref(menuItems[0]);
    
    const selectItem = (item: string) => {
        selectedItem.value = item;
    };

    const enabledModal = ref<"updateMaterial" | "deleteMaterial" | "createMaterial" | null>(null);
    let selectedMaterial = ref<Material | null>(null);
    
    const updateError = ref<string | null>(null);
    const deleteError = ref<string | null>(null);

    const editingMaterial = ref<any>(null);
    
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
</script>

<template>
    <Head>
        <Title>{{ courseSmall?.name ?? "Kurz" }} • Think different Academy</Title>
    </Head>

    <div :class="$style.course">
        <h1 :class="['text-gradient', $style.title]">{{ courseSmall?.name }}</h1>
        <div :class="$style.info">
            <p v-if="coursePending">Načítání kurzu...</p>
            <p v-else>{{ course?.description }}</p>
            <div :class="['liquid-glass', $style.brief]">
                <div :class="$style.fields">
                    <div :class="$style.el">
                        <p :class="$style.title">Počet zhlédnutí</p>
                        <p :class="$style.item">Samen</p>
                    </div>
                    <div :class="$style.el">
                        <p :class="$style.title">Hodnocení</p>
                        <p :class="$style.item">Samen</p>
                    </div>
                    <div :class="$style.el">
                        <p :class="$style.title">Něco dalšího</p>
                        <p :class="$style.item">Samen</p>
                    </div>
                </div>
                <div :class="$style.buttons">
                    <Button button-style="primary" accent-color="primary">Zapsat se</Button>
                    <Button button-style="secondary" accent-color="secondary">Více informací</Button>
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
                <div v-if="selectedItem == 'Materiály'" :class="$style.materials">
                    <Button v-if="user?.uuid == courseSmall?.lecturerUuid" button-style="primary" accent-color="secondary" @click="openCreateMaterialModal" :class="$style.addMaterialButton">
                        Přidat nový materiál
                    </Button>
                    
                    <p v-if="coursePending">Načítání materiálů...</p>
                    <p v-else-if="course?.materials === undefined || course.materials.length == 0">Tento kurz nemá žádné materiály.</p>
                    <ul v-else>
                        <li v-for="material in course.materials" :key="material.uuid">
                            <MaterialItem 
                                :material="material" 
                                :course="course"
                                :edit-mode="user?.uuid == courseSmall?.lecturerUuid"
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


                    >.title {
                        font-size: 16px;
                        margin-bottom: 8px;
                    }
                    >.item {
                        font-size: 24px;
                        margin: 0;
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

            >.buttons {
                display: flex;
                gap: 12px;
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