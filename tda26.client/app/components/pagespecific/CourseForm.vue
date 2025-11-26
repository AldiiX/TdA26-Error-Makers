<script setup lang="ts">
type Material = {
    uuid?: string | null;
    name: string;
    type: "url" | "file";
    url?: string | null;
    file?: File | null;
};

interface CourseFormModel {
    name: string;
    description: string;
    materials: Material[];
}

const props = defineProps<{
    modelValue: CourseFormModel;
    mode?: "create" | "edit";
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: CourseFormModel): void;
    (e: "submit", value: CourseFormModel): void;
    (e: "cancel"): void;
}>();

const updateField = (field: keyof CourseFormModel, value: any) => {
    emit("update:modelValue", { ...props.modelValue, [field]: value });
};

const updateMaterial = (index: number, newData: Partial<Material>) => {
    const materials = [...props.modelValue.materials];
    materials[index] = { ...materials[index], ...newData };
    emit("update:modelValue", { ...props.modelValue, materials });
};

const addMaterial = () => {
    const materials = [...props.modelValue.materials];
    materials.push({ name: "", type: "url", url: "" });
    emit("update:modelValue", { ...props.modelValue, materials });
};

const removeMaterial = (index: number) => {
    const materials = [...props.modelValue.materials];
    materials.splice(index, 1);
    emit("update:modelValue", { ...props.modelValue, materials });
};

const submitForm = () => emit("submit", props.modelValue);
</script>

<template>
    <form @submit.prevent="submitForm" class="course-form">
        <div class="form-group">
            <label>Název</label>
            <input
                type="text"
                :value="modelValue.name"
                @input="updateField('name', $event.target.value)"
                required
            />
        </div>

        <div class="form-group">
            <label>Popis</label>
            <textarea
                :value="modelValue.description"
                @input="updateField('description', $event.target.value)"
                rows="4"
            ></textarea>
        </div>

        <!-- MATERIALS SECTION -->
        <div class="materials">
            <div class="materials-header">
                <label>Materiály</label>
                <button type="button" class="add-btn" @click="addMaterial">+</button>
            </div>

            <div class="material-item" v-for="(m, i) in modelValue.materials" :key="i">
                <input
                    type="text"
                    placeholder="Název materiálu"
                    :value="m.name"
                    @input="updateMaterial(i, { name: $event.target.value })"
                />

                <select
                    :value="m.type"
                    @change="updateMaterial(i, { type: $event.target.value })"
                >
                    <option value="url">URL</option>
                    <option value="file">Soubor</option>
                </select>

                <template v-if="m.type === 'url'">
                    <input
                        type="text"
                        placeholder="Odkaz"
                        :value="m.url"
                        @input="updateMaterial(i, { url: $event.target.value })"
                    />
                </template>

                <template v-else>
                    <input
                        type="file"
                        @change="updateMaterial(i, { file: $event.target.files?.[0] })"
                    />
                </template>

                <button type="button" class="remove-btn" @click="removeMaterial(i)">×</button>
            </div>
        </div>

        <!-- BUTTONS -->
        <div class="form-buttons">
            <button type="button" class="cancel-btn" @click="$emit('cancel')">
                Zrušit
            </button>
            <button type="submit" class="submit-btn">
                {{ mode === 'create' ? 'Vytvořit' : 'Uložit' }}
            </button>
        </div>
    </form>
</template>

<style scoped lang="scss">
.course-form {
    display: flex;
    flex-direction: column;
    gap: 18px;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 6px;

    label {
        font-weight: 600;
    }
}

input,
textarea,
select {
    border: 1px solid var(--background-color-secondary);
    border-radius: 8px;
    padding: 8px 12px;
    background-color: var(--background-color-secondary);
    color: var(--text-color-secondary);
    font-size: 16px;
}

.materials {
    display: flex;
    flex-direction: column;
    gap: 12px;

    .materials-header {
        display: flex;
        justify-content: space-between;
        align-items: center;

        label {
            font-weight: 600;
        }

        .add-btn {
            background-color: var(--accent-color-primary);
            color: white;
            border-radius: 8px;
            padding: 2px 10px;
            font-size: 18px;
            cursor: var(--cursor-pointer);
        }
    }

    .material-item {
        display: grid;
        grid-template-columns: 1fr auto;
        gap: 8px;
        position: relative;

        input,
        select {
            width: 100%;
        }

        .remove-btn {
            position: absolute;
            right: -8px;
            top: -8px;
            background-color: #ff4a4aa2;
            border-radius: 50%;
            width: 22px;
            height: 22px;
            color: white;
            font-size: 16px;
            cursor: var(--cursor-pointer);
        }
    }
}

.form-buttons {
    display: flex;
    justify-content: flex-end;
    gap: 12px;

    .cancel-btn {
        background-color: #aaa2;
        border-radius: 8px;
        padding: 8px 16px;
        font-weight: 600;
    }

    .submit-btn {
        background-color: var(--accent-color-primary);
        color: white;
        border-radius: 8px;
        padding: 8px 16px;
        font-weight: 600;
    }
}
</style>
