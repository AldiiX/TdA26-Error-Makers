<script setup lang="ts">
import type {Course, CourseCategory, CourseFormModel, MaterialFormModel} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import MaterialFormItem from "~/components/pagespecific/MaterialFormItem.vue";
import { push } from "notivue";
import {ref} from "vue";
import CategoryAndTagsSelection from "~/components/pagespecific/CategoryAndTagsSelection.vue";

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
        getBaseUrl() + `/api/v1/courses/${props.courseId}?full=true`,
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
            await $fetch(getBaseUrl() + "/api/v1/courses", { method: "POST", body: fd });
            push.success({
                title: "Kurz vytvořen",
                message: "Kurz byl úspěšně vytvořen.",
                duration: 4000
            });
        } else {
            await $fetch(getBaseUrl() + `/api/v1/courses/${props.courseId}`, { method: "PUT", body: fd });
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

const resetTags = () => {
    form.value.tagsUuid = [];
};

</script>

<template>
    <form :class="$style.courseForm" @submit.prevent="submitForm">
        <div :class="$style.formGroup">
            <label>Název *</label>
            <Input v-model="form.name" type="text" maxlength="128" :disabled="isInProgress" required />
        </div>

        <div :class="$style.formGroup">
            <label>Popis *</label>
            <Input v-model="form.description" type="textarea" rows="4" maxlength="1048" :disabled="isInProgress" required />
        </div>

        <div :class="$style.formGroup">
            <label>Kategorie *</label>
            <Input 
                v-model="form.categoryUuid" 
                type="select"
                :disabled="isInProgress"
                required
                @input="resetTags">
                <option v-for="cat in props.categories" :key="cat.uuid" :value="cat.uuid">{{ cat.label }}</option>
            </Input>
        </div>

        <div :class="$style.formGroup">
            <label>Tagy (max 5)</label>

            <CategoryAndTagsSelection
                v-model="form.tagsUuid"
                :category-uuid="form.categoryUuid"
            />
        </div>

        <div :class="$style.materials">
            <div :class="$style.header">
                <label>Materiály</label>
                <button type="button" :class="$style.add" :disabled="isInProgress" @click="addMaterial">Přidat</button>
            </div>

            <div v-for="(m, i) in form.materials" :key="i" :class="[$style.materialGroup]">
                <MaterialFormItem
:model-value="m" :index="i"
                                  @update:model-value="(val) => updateMaterial(i, val)"
                                  @remove="() => removeMaterial(i)"
                                  @file-selected="handleFileChange" />
            </div>
        </div>

        <p v-if="error" class="error-text">{{ error }}</p>
        <p v-if="loading">Probíhá ukládání...</p>

        <div :class="$style.formButtons">
            <Button button-style="tertiary" type="button" :disabled="loading || isInProgress" @click="emit('finished')">
                Zrušit
            </Button>
            <Button button-style="primary" type="submit" :disabled="loading || isInProgress">
                {{ props.mode === 'create' ? 'Vytvořit' : 'Uložit' }}
            </Button>
        </div>
    </form>
</template>

<style module lang="scss">
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
    
    textarea {
        resize: none;
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
