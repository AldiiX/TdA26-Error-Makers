<script setup lang="ts">
import type {Course, CourseCategory, CourseFormModel, MaterialFormModel} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import MaterialFormItem from "~/components/pagespecific/MaterialFormItem.vue";
import { push } from "notivue";
import {ref} from "vue";

type Material = MaterialFormModel;

const props = defineProps<{
    mode: "create" | "edit";
    courseId?: string | null;
    categories: CourseCategory[];
}>();

const emit = defineEmits<{
    (e: "finished"): void;
}>();

const loading = ref(false);
const error = ref<string | null>(null);

const isInProgress = ref<boolean>(false);

const form = ref<CourseFormModel>({
    name: "",
    description: "",
    materials: [],
    categoryUuid: props.categories[0]!.uuid,
    tagsUuid: []
});

// LOAD EXISTING DATA WHEN EDITING
if (props.mode === "edit" && props.courseId) {
    const { data } = await useFetch<Course>(
        getBaseUrl() + `/api/v2/courses/${props.courseId}?full=true`,
        { server: false }
    );

    const c = data.value;
    if (c) {
        form.value = {
            name: c.name,
            description: c.description ?? "",
            categoryUuid: c.category.uuid,
            tagsUuid: c.tags?.map(t => t.uuid) ?? [],
            materials: (c.materials ?? []).map(m => ({
                uuid: m.uuid,
                name: m.name,
                type: m.type,
                url: m.url ?? null,
                file: null,
                description: m.description ?? null
            }))
        };
    }
}

// FIELD HELPERS
const updateField = (field: keyof CourseFormModel, value: any) => {
    form.value = { ...form.value, [field]: value };
};

const updateMaterial = (index: number, newData: Partial<Material>) => {
    form.value.materials[index] = {
        ...(form.value.materials[index] || { name: "", type: "url", url: null, file: null }),
        ...newData
    } as Material;
};

const addMaterial = () => {
    form.value.materials.push({ name: "", type: "url", url: "", file: null } as Material);
};

const removeMaterial = (index: number) => {
    form.value.materials.splice(index, 1);
};

const handleFileChange = (index: number, file: File | null) => {
    updateMaterial(index, { file });
};

const submitForm = async () => {
    loading.value = true;
    error.value = null;
    isInProgress.value = true;

    try {
        const fd = new FormData();
        fd.append("Course.Name", form.value.name);
        fd.append("Course.Description", form.value.description ?? "");
        fd.append("Course.CategoryUuid", form.value.categoryUuid);
        form.value.tagsUuid.forEach((tagUuid, index) => {
            fd.append(`Course.TagsUuid[${index}]`, tagUuid);
        });

        if (props.mode === "edit") fd.append("Course.Uuid", props.courseId ?? "");

        form.value.materials.filter(m => m.type === "file").forEach((m, index) => {
            if (m.uuid) fd.append(`FileMaterials[${index}].Uuid`, m.uuid);
            fd.append(`FileMaterials[${index}].Name`, m.name);
            fd.append(`FileMaterials[${index}].Type`, "file");
            fd.append(`FileMaterials[${index}].Description`, m.description ?? "");
            if (m.file) fd.append(`FileMaterials[${index}].File`, m.file);
        });

        form.value.materials.filter(m => m.type === "url").forEach((m, index) => {
            if (m.uuid) fd.append(`UrlMaterials[${index}].Uuid`, m.uuid);
            fd.append(`UrlMaterials[${index}].Name`, m.name);
            fd.append(`UrlMaterials[${index}].Type`, "url");
            fd.append(`UrlMaterials[${index}].Description`, m.description ?? "");
            fd.append(`UrlMaterials[${index}].Url`, m.url ?? "");
        });

        if (props.mode === "create") {
            await $fetch(getBaseUrl() + "/api/v2/courses", { method: "POST", body: fd });
            push.success({
                title: "Kurz vytvořen",
                message: "Kurz byl úspěšně vytvořen.",
                duration: 4000
            });
        } else {
            await $fetch(getBaseUrl() + `/api/v2/courses/${props.courseId}`, { method: "PUT", body: fd });
            push.success({
                title: "Kurz upraven",
                message: "Kurz byl úspěšně upraven.",
                duration: 4000
            });
        }

        emit("finished");

    } catch (err: any) {
        error.value = err?.data?.error ?? err?.data?.message ?? err?.message ?? "Server error";
    } finally {
        loading.value = false;
        isInProgress.value = false;
    }
};

type Tag = { uuid: string; displayName: string };

const selectedTags = ref<Tag[]>([]);
const tagQuery = ref("");

const { data: allTags } = await useFetch<Tag[]>(
    getBaseUrl() + "/api/v2/course-tags?categoryUuid=" + form.value.categoryUuid,
    { server: false }
);

const filteredTags = computed(() => {
    if (!tagQuery.value) return []
    
    return allTags.value
        ?.filter(t =>
            t.displayName.toLowerCase().includes(tagQuery.value.toLowerCase()) &&
            !selectedTags.value.some(st => st.uuid === t.uuid)
        )
        .slice(0, 5);
})

const addTag = (tag: Tag) => {
    if (selectedTags.value.length >= 5) return;
    selectedTags.value.push(tag);
    tagQuery.value = "";
    syncTags();
}

const addCustomTag = async () => {
    if (!tagQuery.value || selectedTags.value.length >= 5) return;

    const tempUuid = `temp-${crypto.randomUUID()}`;

    const optimisticTag: Tag = {
        uuid: tempUuid,
        displayName: tagQuery.value
    }

    selectedTags.value.push(optimisticTag);
    tagQuery.value = "";
    syncTags();

    try {
        const newTag = await $fetch<any>(
            getBaseUrl() + "/api/v2/course-tags",
            {
                method: "POST",
                body: {
                    displayName: optimisticTag.displayName,
                    categoryUuid: form.value.categoryUuid
                }
            }
        );

        const idx = selectedTags.value.findIndex(t => t.uuid === tempUuid);
        if (idx !== -1) {
            selectedTags.value[idx]!.uuid = newTag.uuid;
            syncTags();
        }

    } catch (err) {
        selectedTags.value = selectedTags.value.filter(t => t.uuid !== tempUuid);
        syncTags();
        throw err;
    }
}


const syncTags = () => {
    form.value.tagsUuid = selectedTags.value.map(t => t.uuid);
}

const removeTag = (uuid: string) => {
    selectedTags.value = selectedTags.value.filter(t => t.uuid !== uuid);
    syncTags();
}

</script>

<template>
    <form @submit.prevent="submitForm" :class="$style.courseForm">
        <div :class="$style.formGroup">
            <label>Název *</label>
            <Input type="text" v-model="form.name" maxlength="128" :disabled="isInProgress" required />
        </div>

        <div :class="$style.formGroup">
            <label>Popis *</label>
            <Input type="textarea" v-model="form.description" rows="4" maxlength="1048" :disabled="isInProgress" required />
        </div>

        <div :class="$style.formGroup">
            <label>Kategorie *</label>
            <Input type="select" v-model="form.categoryUuid" rows="4" maxlength="1048" :disabled="isInProgress" required>
                <option v-for="cat in props.categories" :key="cat.uuid" :value="cat.uuid">{{ cat.label }}</option>
            </Input>
        </div>

        <div :class="$style.formGroup">
            <label>Tagy (max 5)</label>

            <div :class="$style.tagInput">
                <span
                    v-for="tag in selectedTags"
                    :key="tag.uuid"
                    :class="$style.tag"
                    @click="removeTag(tag.uuid)"
                >
                  <p>{{ tag.displayName }}</p>
                </span>

                <div v-if="selectedTags.length < 5">
                    <Input
                        v-model="tagQuery"
                        :disabled="isInProgress || selectedTags.length >= 5"
                        placeholder="Začni psát tag..."
                        @keydown.enter.prevent="addCustomTag"
                        maxlength="32"
                        @blur="tagQuery = ''"
                    />

                    <div v-if="tagQuery !== ''" :class="$style.suggestions">
                        <ul>
                            <li
                                key="new"
                                @mousedown.prevent="addCustomTag"
                            >
                                Přidat „{{ tagQuery }}”
                            </li>
                            <hr v-if="filteredTags && filteredTags?.length > 0" />
                            <li
                                v-for="tag in filteredTags"
                                :key="tag.uuid"
                                @mousedown.prevent="addTag(tag)"
                            >
                                {{ tag.displayName }}
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>

        <div :class="$style.materials">
            <div :class="$style.header">
                <label>Materiály</label>
                <button type="button" @click="addMaterial" :class="$style.add" :disabled="isInProgress">Přidat</button>
            </div>

            <div v-for="(m, i) in form.materials" :key="i" :class="[$style.materialGroup]">
                <MaterialFormItem :model-value="m" :index="i"
                                  @update:modelValue="(val) => updateMaterial(i, val)"
                                  @remove="() => removeMaterial(i)"
                                  @file-selected="handleFileChange" />
            </div>
        </div>

        <p v-if="error" class="error-text">{{ error }}</p>
        <p v-if="loading">Probíhá ukládání...</p>

        <div :class="$style.formButtons">
            <Button button-style="tertiary" type="button" @click="emit('finished')" :disabled="loading || isInProgress">
                Zrušit
            </Button>
            <Button button-style="primary" type="submit" :disabled="loading || isInProgress">
                {{ props.mode === 'create' ? 'Vytvořit' : 'Uložit' }}
            </Button>
        </div>
    </form>
</template>

<style module lang="scss">
.tagInput {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    position: relative;
    align-items: center;
    
    >div {
        position: relative;
        flex: 1;
        width: 200px;
        display: flex;
        
        
        .suggestions {
            margin-top: 6px;
            position: absolute;
            top: 100%;
            left: 0;
            z-index: 1000;
            border-radius: 12px;
            width: inherit;
            border: 1px solid rgb(from var(--background-color-secondary) r g b / 1);
            
            ul {
                background-color: var(--background-color-secondary);
                border-radius: 12px;
                box-shadow: 0 4px 24px rgba(0, 0, 0, 0.15);
                backdrop-filter: blur(8px);
                max-height: 150px;
                display: flex;
                flex-direction: column;
                overflow-x: hidden;
                overflow-y: auto;
                width: inherit;

                hr {
                    margin: 4px 0;
                    border: none;
                    border-top: 1px solid rgb(from var(--background-color-primary) r g b / 1);
                }

                input {
                    width: inherit;
                }

                li {
                    padding: 6px 24px;
                    cursor: pointer;
                    width: inherit;
                    overflow: hidden;
                    text-overflow: ellipsis;
                    white-space: nowrap;
                    min-height: 32.5px;

                    &:hover {
                        background: var(--background-color-primary);
                    }
                }

                &::-webkit-scrollbar {
                    width: 8px;
                }

                &::-webkit-scrollbar-track {
                    background: transparent;
                }

                &::-webkit-scrollbar-thumb {
                    background: var(--scrollbar-color);
                    border-radius: 4px;
                }
            }
        }
    }
}

.tag {
    background: var(--accent-color-primary);
    color: white;
    padding: 12px 16px;
    border-radius: 12px;
    cursor: pointer;
    height: fit-content;
    display: flex;
    align-items: center;
    
    p {
        margin: 0;
        display: flex;
        align-items: center;
        
        &::after {
            content: '';
            margin-left: 8px;
            mask-image: url("../../../public/icons/x.svg");
            mask-size: cover;
            display: inline-block;
            width: 12px;
            height: 12px;
            background-color: white;
        }
    }
}

.courseForm {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.formGroup {
    display: flex;
    flex-direction: column;
    gap: 6px;
    position: relative;

    label {
        font-weight: 600;
    }
}

*:disabled {
    cursor: not-allowed;
}

.materials {
    display: flex;
    flex-direction: column;
    gap: 20px;

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;

        label {
            font-weight: 600;
        }

    }

    button {
        background: none;
        border: none;
        cursor: pointer;
        color: var(--accent-color-primary);

        &::before {
            mask-size: cover;
            background-color: var(--accent-color-primary);
            display: inline-block;
            width: 12px;
            height: 12px;
            margin-right: 4px;
        }
    }

    .remove {
        font-size: 16px;
        font-weight: 600;
    }

    .add {
        font-size: 16px;
        font-weight: 600;

        &::before {
            content: '';
            mask-image: url("../../../public/icons/plus.svg");
        }

        &:disabled {
            opacity: .5;
            cursor: not-allowed;
        }
    }
}

.formButtons {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    margin-top: 32px;

    button {
        width: 164px;
    }
}

.materialGroup {
    display: flex;
    flex-direction: column;
    gap: 8px;
    border-radius: 8px;
    padding: 12px;
    background-color: var(--background-color-secondary);
}
</style>
