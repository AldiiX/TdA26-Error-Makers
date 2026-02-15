<script setup lang="ts">
import { computed } from "vue";
import type {MaterialFormModel} from "#shared/types";
import Input from "~/components/Input.vue";

const props = withDefaults(defineProps<{
    modelValue: MaterialFormModel;
    index: number;
    showRemoveButton?: boolean;
}>(), {
    showRemoveButton: true,
});

const emit = defineEmits<{
    (e: "update:modelValue", value: MaterialFormModel): void;
    (e: "remove", index?: number): void;
    (e: "file-selected", index: number, file: File): void;
}>();

const m = computed({
    get: () => props.modelValue,
    set: (val) => emit("update:modelValue", val),
});

function onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.[0]) {
        emit("file-selected", props.index, input.files[0]);
    }
}
</script>

<template>
    <div :class="$style.item">
        <div :class="$style.headInputs">
            <Input v-model="m.name" type="text" placeholder="Název materiálu *" maxlength="128" required/>

            <Input v-model="m.type" type="select">
                <option value="url">URL</option>
                <option value="file">Soubor</option>
            </Input>
        </div>

        <template v-if="m.type === 'url'">
            <Input v-model="m.url" type="text" placeholder="Odkaz *" maxlength="256" required />
        </template>

        <template v-else>
            <Input
                type="file"
                :allowed-file-types="[
                    'application/pdf',
                    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
                    'text/plain',
                    'image/png','image/jpeg','image/gif',
                    'video/mp4','audio/mpeg'
                ]"
                @change="onFileChange"
            />
        </template>

        <Input
            v-model="m.description"
            type="textarea"
            placeholder="Popis"
            rows="4"
            maxlength="1048"
            style="resize: none"
        />

        <button v-if="showRemoveButton" :class="$style.remove" type="button" @click="emit('remove', props.index)">
            Odstranit
        </button>
    </div>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.item {
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: center;
    gap: 16px;

    :first-child {
        width: 100%;
    }
    .headInputs {
        display: flex;
        gap: 8px;
        width: 100%;

        :first-child {
            flex: 8;
        }

        :last-child {
            flex: 1;
        }
    }
    
    >* {
        width: 100%;
    }
    
    .remove {
        background-color: transparent;
        border: none;
        color: var(--accent-color-primary);
        font-weight: 600;
        cursor: pointer;
    }
}


/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
    .item {
        .headInputs {
            flex-direction: column;

            :first-child,
            :last-child {
                flex: unset;
                width: 100%;
            }
        }
    }
}
</style>
