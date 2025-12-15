<script setup lang="ts">
import type {Question} from "#shared/types";
import Button from "~/components/Button.vue";

const props = defineProps<{
    question: Question,
    editMode?: boolean
}>();

const emit = defineEmits<{
    (e: 'update:question', value: Partial<Question>): void;
    (e: 'update:selectedOption', selectedIndices: number[]): void;
}>();

const updateQuestionText = (e: Event) => {
    const text = (e.target as HTMLElement).innerText;
    emit('update:question', {
        question: text,
    });
};

const updateOptionText = (index: number, e: Event) => {
    const text = (e.target as HTMLElement).innerText;
    const options = [...props.question.options];
    options[index] = text;

    emit('update:question', {
        options,
    });
};

const selectedIndices = ref<number[]>([]);

const selectOption = (index: number) => {
    const currentType = props.question.type;
    
    if (props.editMode) {
        if (selectedIndices.value.includes(index)) {
            selectedIndices.value = selectedIndices.value.filter(i => i !== index);
        } else {
            selectedIndices.value.push(index);
        }
    } else {
        switch (currentType) {
            case "singleChoice":
                if (selectedIndices.value.includes(index)) {
                    selectedIndices.value = selectedIndices.value.filter(i => i !== index);
                    return;
                }

                selectedIndices.value = [index];
                break;
            case "multipleChoice":
                if (selectedIndices.value.includes(index)) {
                    selectedIndices.value = selectedIndices.value.filter(i => i !== index);
                } else {
                    selectedIndices.value.push(index);
                }
                break;
            default:
                console.warn("Unknown question type:", currentType);
        }
    }
    
    emitSelectionUpdate();
};

const syncFromQuestion = () => {
    if (!props.editMode) return;
    
    if (props.question.type === "singleChoice" && props.question.correctIndex != null) {
        selectedIndices.value = [props.question.correctIndex];
        return;
    }

    if (props.question.type === "multipleChoice") {
        selectedIndices.value = [...(props.question.correctIndices ?? [])];
        return;
    }

    selectedIndices.value = [];
};

watch(
    () => props.question,
    () => {
        selectedIndices.value = [];
        syncFromQuestion();
    },
    { immediate: true, deep: true }
);

const emitSelectionUpdate = () => {
    const count = selectedIndices.value.length;
    const prevType = props.question.type;
    
    emit('update:selectedOption', [...selectedIndices.value]);

    if (count === 1) {
        const nextType = "singleChoice";

        emit("update:question", {
            uuid: prevType !== nextType ? undefined : props.question.uuid,
            type: nextType,
            correctIndices: [...selectedIndices.value],
        });
        return;
    }

    if (count > 1) {
        const nextType = "multipleChoice";

        emit("update:question", {
            uuid: prevType !== nextType ? undefined : props.question.uuid,
            type: nextType,
            correctIndices: [...selectedIndices.value],
        });
        return;
    }

    // 0 selected
    emit("update:question", {
        ...props.question,
        correctIndex: undefined,
        correctIndices: [],
    });
};

emitSelectionUpdate();
</script>

<template>
    <div :class="[editMode && $style.editMode, $style.questionContainer]">
        <div :class="$style.title">
            <p 
                :class="[$style.editable]" 
                :contenteditable="editMode"
                @input="editMode && updateQuestionText($event)"
            >{{ question.question }}</p>
        </div>
        <ul :key="question.uuid">
            <li v-for="(option, index) in question.options" :key="index">
                <Button
                    :button-style="selectedIndices.includes(index) ? 'primary' : 'tertiary'"
                    @click="selectOption(index)"
                    :class="[$style.editable]"
                    :contenteditable="editMode"
                    @input="editMode && updateOptionText(index, $event)"
                >{{ option }}</Button>
            </li>
        </ul>
    </div>
</template>

<style module lang="scss">
@use "../../app" as *;

.editMode {
    .editable {
        @include editable;
    }
}

.questionContainer {
    padding: 16px;
    border-radius: 8px;
    background-color: var(--background-color-secondary);
    height: 300px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-direction: column;
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);

    .title {
        display: flex;
        align-items: center;
        justify-content: center;
        flex: 1;
        text-align: center;
        max-width: 90%;

        p {
            font-size: 32px;
        }
    }

    ul {
        list-style: none;
        display: grid;
        gap: 12px 16px;
        margin: 16px 0 0;
        grid-template-columns: repeat(2, 1fr);
        width: 100%;
        padding: 0 16px;

        li {
            display: flex;
            justify-content: center;

            button {
                width: 100%;
                max-width: 210px;
                border: none !important;
            }
        }
    }
}
</style>