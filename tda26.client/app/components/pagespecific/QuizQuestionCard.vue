<script setup lang="ts">
import type {Question} from "#shared/types";
import Button from "~/components/Button.vue";
import Modal from "~/components/Modal.vue";

const props = defineProps<{
    question: Question,
    editMode?: boolean,
    selectedOption?: number[]
}>();

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
    if (!props.editMode) {
        selectedIndices.value = props.selectedOption ?? [];
        return
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
    () => props.question,
    () => {
        console.log("Question changed to:", props.question);
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

const isDeleteModalOpen = ref(false);
</script>

<template>
    <div :class="[editMode && $style.editMode, $style.questionContainer]">
        <span 
            v-if="editMode"
            :class="$style.deleteButton"
            @click="isDeleteModalOpen = true"
        ></span>
        <div :class="$style.title">
            <p 
                :class="[$style.editable]" 
                :contenteditable="editMode"
                @input="editMode && updateQuestionText($event)"
            >{{ question.question }}</p>
        </div>
        <ul :key="question.options.length">
            <li v-for="(option, index) in question.options" :key="index">
                <Button
                    :button-style="selectedIndices.includes(index) ? 'primary' : 'tertiary'"
                    @click="selectOption(index)"
                    :class="[$style.editable]"
                    :contenteditable="editMode"
                    @input="editMode && updateOptionText(index, $event)"
                >{{ option }} <span 
                    :class="$style.removeOption"
                    v-if="editMode"
                    @click="emit('removeQuestionOption', index)"
                ></span></Button>
            </li>
            <li v-if="editMode && question.options.length < 6">
                <Button
                    button-style="tertiary"
                    @click="emit('addQuestionOption')"
                    :class="$style.addOptionButton"
                ></Button>
            </li>
        </ul>
    </div>
    
    <Modal 
        v-if="editMode"
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

            button {
                width: 100%;
                max-width: 210px;
                min-height: 40px;
                border: none !important;
                display: flex;
                align-items: center;
                justify-content: center;
                
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