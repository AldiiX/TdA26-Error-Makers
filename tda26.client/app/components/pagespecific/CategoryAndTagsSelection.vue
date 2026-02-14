<script setup lang="ts">
import { ref, computed, watch } from "vue";
import Input from "~/components/Input.vue";
import getBaseUrl from "#shared/utils/getBaseUrl";

type Tag = {
    uuid: string;
    displayName: string;
};

const props = defineProps<{
    modelValue: string[];
    categoryUuid: string | null;
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: string[]): void;
}>();

const selectedTags = ref<Tag[]>([]);
const tagQuery = ref("");
const allTags = ref<Tag[]>([]);

watch(
    () => props.modelValue,
    (val) => {
        if (selectedTags.value.length === 0 && val.length > 0) {
            selectedTags.value = allTags.value.filter(t =>
                val.includes(t.uuid)
            );
        }
    },
    { immediate: true }
);

watch(
    () => props.categoryUuid,
    async (uuid) => {
        if (!uuid) {
            allTags.value = [];
            selectedTags.value = [];
            return;
        }

        const tags = await $fetch<Tag[]>(
            getBaseUrl() + "/api/v2/course-tags",
            {
                query: { categoryUuid: uuid }
            }
        );

        allTags.value = tags;

        if (selectedTags.value.length === 0 && props.modelValue.length > 0) {
            selectedTags.value = tags.filter(t =>
                props.modelValue.includes(t.uuid)
            );
        }
    },
    { immediate: true }
);


const filteredTags = computed(() => {
    if (!tagQuery.value) return [];

    return allTags.value
        .filter(t =>
            t.displayName.toLowerCase().includes(tagQuery.value.toLowerCase()) &&
            !selectedTags.value.some(st => st.uuid === t.uuid)
        )
        .slice(0, 5);
});

const syncTags = () => {
    emit(
        "update:modelValue",
        selectedTags.value.map(t => t.uuid)
    );
};

const addTag = (tag: Tag) => {
    if (selectedTags.value.length >= 5) return;

    selectedTags.value.push(tag);
    tagQuery.value = "";
    syncTags();
};

const addCustomTag = async () => {
    if (!tagQuery.value || selectedTags.value.length >= 5) return;

    const tempUuid = `temp-${crypto.randomUUID()}`;

    const optimisticTag: Tag = {
        uuid: tempUuid,
        displayName: tagQuery.value
    };

    addTag(optimisticTag);

    try {
        const newTag = await $fetch<Tag>(
            getBaseUrl() + "/api/v2/course-tags",
            {
                method: "POST",
                body: {
                    displayName: optimisticTag.displayName,
                    categoryUuid: props.categoryUuid
                }
            }
        );

        const idx = selectedTags.value.findIndex(t => t.uuid === tempUuid);
        if (idx !== -1) {
            selectedTags.value[idx] = newTag;
            syncTags();
        }
    } catch (err) {
        selectedTags.value = selectedTags.value.filter(t => t.uuid !== tempUuid);
        syncTags();
        throw err;
    }
};

const removeTag = (uuid: string) => {
    selectedTags.value = selectedTags.value.filter(t => t.uuid !== uuid);
    syncTags();
};
</script>

<template>
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
                placeholder="Začni psát tag…"
                maxlength="32"
                @keydown.enter.prevent="addCustomTag"
                @blur="tagQuery = ''"
            />

            <div v-if="tagQuery !== ''" :class="$style.suggestions">
                <ul>
                    <li @mousedown.prevent="addCustomTag">
                        Přidat „{{ tagQuery }}“
                    </li>

                    <hr v-if="filteredTags.length > 0" />

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
                overflow-y: auto;

                hr {
                    margin: 4px 0;
                    border: none;
                    border-top: 1px solid rgb(from var(--background-color-primary) r g b / 1);
                }

                li {
                    padding: 6px 24px;
                    cursor: pointer;
                    white-space: nowrap;

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
    display: flex;
    align-items: center;
    transition-duration: 0.3s;

    &:hover {
        background: var(--accent-color-primary-darker);
        transition-duration: 0.3s;
    }

    p {
        margin: 0;

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
</style>
