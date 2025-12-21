<script setup lang="ts">
import type {Question} from "#shared/types";
import Button from "~/components/Button.vue";
import Modal from "~/components/Modal.vue";

const props = withDefaults(defineProps<{
    question: Question,
    mode?: "play" | "edit" | "result",
    selectedOption?: number[]
}>(), {
    mode: "play",
});

const emit = defineEmits<{
    (e: 'update:question', value: Partial<Question>): void;
    (e: 'update:selectedOption', selectedIndices: number[]): void;
    (e: 'deleteQuestion', question: Question): void;
    (e: 'addQuestionOption'): void;
    (e: 'removeQuestionOption', index: number): void;
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

const selectedIndices = ref<number[]>(props.selectedOption ?? []);

const selectOption = (index: number) => {
    if (props.mode === 'result') return;
    
    const currentType = props.question.type;
    
    if (props.mode === 'edit') {
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
    if (props.mode !== 'edit') {
        selectedIndices.value = props.selectedOption ?? [];
        return;
    }
    
    if (props.question.type === "singleChoice" && props.question.correctIndex != null) {
        selectedIndices.value = [props.question.correctIndex];
        return;
    }

    if (props.question.type === "multipleChoice") {
        selectedIndices.value = [...(props.question.correctIndices ?? [])];
        return;
    }

    selectedIndices.value = props.selectedOption ?? [];
};

watch(
    () => props.question.uuid,
    () => {
        selectedIndices.value = [];
        syncFromQuestion();
    },
    { immediate: true }
);

const emitSelectionUpdate = () => {
    if (props.mode === 'result') return;
    
    const count = selectedIndices.value.length;
    const prevType = props.question.type;
    
    emit('update:selectedOption', [...selectedIndices.value]);

    if (count === 1) {
        const nextType = "singleChoice";
        console.log("Emitting update for singleChoice");

        emit("update:question", {
            uuid: prevType !== nextType ? undefined : props.question.uuid,
            type: nextType,
            correctIndex: selectedIndices.value[0],
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

    console.log("Emitting update for 0 selected", selectedIndices.value, props.question);
    
    // 0 selected
    emit("update:question", {
        correctIndex: undefined,
        correctIndices: [],
    });
};

emitSelectionUpdate();

const isDeleteModalOpen = ref(false);
</script>

<template>
    <div :class="[
        mode === 'edit' && $style.editMode, 
        mode === 'result' && $style.resultMode, $style.questionContainer,
        (mode === 'result' && question.isCorrect) ? $style.correct : $style.incorrect
        ]">
        <span 
            v-if="mode === 'edit'"
            :class="$style.deleteButton"
            @click="isDeleteModalOpen = true"
        ></span>
        <div :class="$style.title">
            <p 
                :class="[$style.editable]" 
                :contenteditable="mode === 'edit'"
                @input="mode === 'edit' && updateQuestionText($event)"
            >{{ question.question }}</p>
        </div>
        <ul :key="question.options.length">
            <li v-for="(option, index) in question.options" :key="index">
                <div
                    v-if="mode === 'edit'"
                    @click="selectOption(index)"
                    :class="[
                        $style.editable,
                        selectedIndices.includes(index) && $style.selected
                        ]"
                    :contenteditable="mode === 'edit'"
                    @input="mode === 'edit' && updateOptionText(index, $event)"
                >{{ option }} <span
                    :class="$style.removeOption"
                    @click="emit('removeQuestionOption', index)"
                ></span></div>
                <Button
                    v-else
                    :button-style="selectedIndices.includes(index) && mode !== 'result' ? 'primary' : 'tertiary'"
                    @click="selectOption(index)"
                    :class="[
                            $style.editable, 
                            mode === 'result' && question.selectedIndices?.includes(index) && (question.correctIndices?.includes(index) || question.correctIndex === index) && $style.selectedCorrectAnswer,
                            mode === 'result' && question.selectedIndices?.includes(index) && (!question.correctIndices?.includes(index) && question.correctIndex !== index) && $style.selectedIncorrectAnswer,
                            mode === 'result' && !question.selectedIndices?.includes(index) && (question.correctIndices?.includes(index) || question.correctIndex === index) && $style.missedCorrectAnswer
                        ]"
                >{{ option }}</Button>
            </li>
            <li v-if="mode === 'edit' && question.options.length < 6">
                <Button
                    button-style="tertiary"
                    @click="emit('addQuestionOption')"
                    :class="$style.addOptionButton"
                ></Button>
            </li>
        </ul>
    </div>
    
    <Modal 
        v-if="mode === 'edit'"
        :enabled="isDeleteModalOpen"
        @close="isDeleteModalOpen = false"
        can-be-closed-by-clicking-outside
    >
        <h3>Opravdu si přeješ smazat tuto otázku?</h3>
        <p>Tuto akci nelze vrátit zpět.</p>
        <div :class="$style.modalButtons">
            <Button button-style="tertiary" @click="isDeleteModalOpen = false">Zrušit</Button>
            <Button
                button-style="primary"
                accent-color="secondary"
                @click="emit('deleteQuestion', question); isDeleteModalOpen = false"
            >
                Smazat otázku
            </Button>
        </div>
    </Modal>
</template>

<style module lang="scss">
@use "../../app" as *;

.editMode {
    .editable {
        @include editable;
    }
}

.resultMode {
    .editable {
        //pointer-events: none;
    }
    
    button {
        cursor: default;
        
        &:hover {
            opacity: 1 !important;
            background-color: inherit !important;
        }
    }
    
    .missedCorrectAnswer {
        background-color: color-mix(in srgb, var(--color-success) 10%, var(--background-color-secondary) 90%) !important;
        border: 2px dashed var(--color-success) !important;
        color: var(--color-success-text) !important;
        opacity: 0.85;
    }
    
    .selectedIncorrectAnswer {
        background-color: color-mix(in srgb, var(--color-error) 20%, var(--background-color-secondary) 80%) !important;
        border: 2px solid var(--color-error) !important;
        color: var(--color-error-text) !important;
    }

    .selectedCorrectAnswer {
        background-color: color-mix(in srgb, var(--color-success) 20%, var(--background-color-secondary) 80%) !important;
        border: 2px solid var(--color-success) !important;
        color: var(--color-success-text) !important;
    }
}

.modalButtons {
    display: flex;
    gap: 16px;
    margin-top: 24px;
}

.questionContainer {
    padding: 16px;
    border-radius: 8px;
    background-color: var(--background-color-secondary);
    min-height: 300px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-direction: column;
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);
    position: relative;
    
    .deleteButton {
        position: absolute;
        top: 16px;
        right: 16px;
        width: 32px;
        height: 32px;
        background-color: var(--color-error);
        mask-image: url('/icons/trash.svg');
        mask-size: cover;
        mask-position: center;
        mask-repeat: no-repeat;
        cursor: pointer;
        transition: opacity 0.2s;

        &:hover {
            opacity: 1;
        }
    }

    .title {
        display: flex;
        align-items: center;
        justify-content: center;
        flex: 1;
        text-align: center;
        max-width: 90%;
        position: relative;
        z-index: 5;

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
        position: relative;
        z-index: 5;

        li {
            display: flex;
            justify-content: center;

            button, >div {
                width: 100%;
                max-width: 210px;
                min-height: 40px;
                border: none !important;
                display: flex;
                align-items: center;
                justify-content: center;
                
                &.selected {
                    background-color: var(--accent-color-primary);
                    color: var(--accent-color-primary-text);
                }
                
                .removeOption {
                    display: inline-block;
                    width: 16px;
                    height: 16px;
                    background-color: var(--text-color-secondary);
                    mask-image: url('/icons/x.svg');
                    mask-size: cover;
                    mask-position: center;
                    mask-repeat: no-repeat;
                    cursor: pointer;
                    vertical-align: middle;
                    opacity: 0.6;
                    margin-left: auto;

                    &:hover {
                        opacity: 1;
                    }
                }
            }
        }
        
        .addOptionButton::before {
            content: '';
            mask-image: url('/icons/plus.svg');
            mask-size: cover;
            mask-position: center;
            mask-repeat: no-repeat;
            width: 24px;
            height: 24px;
            display: inline-block;
            background-color: var(--text-color-secondary);
            position: absolute;
        }
    }
}
</style>