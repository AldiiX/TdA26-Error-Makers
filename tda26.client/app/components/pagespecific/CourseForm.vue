<script setup lang="ts">
import type {Course, CourseFormModel, MaterialFormModel} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import Input from "~/components/Input.vue";

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

// Typed file change handler to avoid EventTarget typing issues
const handleFileChange = (index: number, event: Event) => {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0] ?? null;
    updateMaterial(index, { file });
};

// SUBMIT
const submitForm = async () => {
    loading.value = true;
    error.value = null;

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
        } else {
            await $fetch(getBaseUrl() + `/api/v2/courses/${props.courseId}`, { method: "PUT", body: fd });
        }

        emit("finished");

    } catch (err: any) {
        error.value = err?.data?.message ?? err?.message ?? "Server error";
    } finally {
        loading.value = false;
    }
};
</script>

<template>
    <form @submit.prevent="submitForm" :class="$style.courseForm">
        <div :class="$style.formGroup">
            <label>Název</label>
            <Input type="text" v-model="form.name" required />
        </div>

        <div :class="$style.formGroup">
            <label>Popis</label>
            <Input type="textarea" v-model="form.description" rows="4"/>
        </div>

        <div :class="$style.materials">
            <div :class="$style.header">
                <label>Materiály</label>
                <button type="button" @click="addMaterial" :class="$style.add">Přidat</button>
            </div>

            <div v-for="(m, i) in form.materials" :key="i" :class="[$style.materialGroup]">
                <div>
                    <Input type="text" placeholder="Název materiálu" v-model="m.name" />
                    <Input type="select" v-model="m.type" :value="m.type">
                        <option value="url">URL</option>
                        <option value="file">Soubor</option>
                    </Input>
                </div>

                <template v-if="m.type === 'url'">
                    <Input type="text" placeholder="Odkaz" v-model="m.url" />
                </template>

                <template v-else>
                    <Input type="file" @change="handleFileChange(i, $event)" defaultFileName="Žádný soubor pro přepsání" />
                </template>

                <Input type="textarea" placeholder="Popis" v-model="m.description" rows="4"/>

                <button type="button" :class="$style.remove" @click="removeMaterial(i)">Odstranit</button>
            </div>
        </div>

        <p v-if="error" class="error-text">{{ error }}</p>
        <p v-if="loading">Probíhá ukládání...</p>

        <div :class="$style.formButtons">
            <Button button-style="tertiary" type="button" @click="emit('finished')" :disabled="loading">
                Zrušit
            </Button>
            <Button button-style="primary" type="submit" :disabled="loading">
                {{ props.mode === 'create' ? 'Vytvořit' : 'Uložit' }}
            </Button>
        </div>
    </form>
</template>

<style module lang="scss">
.courseForm {
    display: flex;
    flex-direction: column;
    gap: 18px;
}

.formGroup {
    display: flex;
    flex-direction: column;
    gap: 6px;

    label {
        font-weight: 600;
    }
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
    }
}

.formButtons {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}

.materialGroup {
    display: flex;
    flex-direction: column;
    gap: 8px;
    border-radius: 8px;
    padding: 12px;
    background-color: var(--background-color-secondary);
    
    >div {
        flex: 1;
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 8px;
        
        :first-child {
            width: 70%;
        }
    }
}
</style>
