<script setup lang="ts">
import type {Course, CourseFormModel, MaterialFormModel} from "#shared/types";
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
    materials: []
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
.courseForm {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.formGroup {
    display: flex;
    flex-direction: column;
    gap: 6px;

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

    button {
        width: 124px;
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
