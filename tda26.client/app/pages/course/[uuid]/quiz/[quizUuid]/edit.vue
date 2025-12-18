<script setup lang="ts">
import { Head, Title } from '#components';
import type {Course, Question, Quiz} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import QuizQuestionCard from "~/components/pagespecific/QuizQuestionCard.vue";

definePageMeta({
    layout: "normal-page-layout"
});

const { uuid, quizUuid } = useRoute().params;

const { data: quiz, pending: quizPending, error: quizError } = await useFetch<Quiz>(() => getBaseUrl() + `/api/v1/courses/${uuid}/quizzes/${quizUuid}`, {
    key: `course-${uuid}-quiz-${quizUuid}`,
});

if (quizError.value) {
    console.error("Error fetching quiz:", quizError.value);
    await navigateTo(`/course/${uuid}`);
}

const oldQuiz = ref<Quiz | null>(null);
watch(quiz, (newQuiz) => {
    if (newQuiz) {
        oldQuiz.value = JSON.parse(JSON.stringify(newQuiz));
    }
}, { immediate: true });

const kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex = ref(0);

const mapQuestionToDto = (q: Question) => {
    if (q.type === "singleChoice") {
        return {
            ...(q.uuid ? { uuid: q.uuid } : {}),
            type: "singleChoice",
            question: q.question,
            options: q.options,
            correctIndex: q.correctIndex!,
        };
    }

    if (q.type === "multipleChoice") {
        return {
            ...(q.uuid ? { uuid: q.uuid } : {}),
            type: "multipleChoice",
            question: q.question,
            options: q.options,
            correctIndices: q.correctIndices ?? [],
        };
    }

    throw new Error("Unknown question type");
};

const saveQuiz = async () => {
    if (!quiz.value || !canSave) return;

    console.log("Saving quiz:", quiz.value);
    
    const dto = {
        uuid: quiz.value!.uuid,
        title: quiz.value!.title,
        attemptsCount: quiz.value!.attemptsCount,
        questions: quiz.value!.questions.map((q, index) => ({
            ...mapQuestionToDto(q),
            order: index,
        })),
    };

    await $fetch(getBaseUrl() + `/api/v1/courses/${uuid}/quizzes/${quizUuid}`, {
        method: 'PUT',
        body: dto,
    });
    
    oldQuiz.value = JSON.parse(JSON.stringify(quiz.value));
    alert("Kvíz byl úspěšně uložen.");
};

const updateQuestion = (i: number, patch: Partial<Question>) => {    
    if (!quiz.value) return;

    quiz.value.questions.splice(i, 1, {
        ...quiz.value.questions[i],
        ...patch,
    } as Question);

    const q = quiz.value.questions[i];
    if (!q) return;
    
    canSave.value = !(
        q.correctIndex === undefined &&
        (!q.correctIndices || q.correctIndices.length === 0)
    );
};

const currentQuestion = computed(() =>
    quiz.value?.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value]
);

const canSave = ref(false);

const updateTitle = (newTitle: string) => {
    if (!quiz.value) return;
    
    quiz.value.title = newTitle;
};

const incrementQuestion = (i: number) => {
    if (!quiz.value) return;
    
    const newIndex = kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value + i;
    if (newIndex < 0) return;
    if (newIndex >= quiz.value.questions.length) return;
    
    kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = newIndex;
};

const setQuestionIndex = (i: number) => {
    if (!quiz.value) return;
    if (i < 0 || i >= quiz.value.questions.length) return;

    kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = i;
};

const addQuestion = () => {
    if (!quiz.value) return;

    quiz.value.questions.push({
        type: "singleChoice",
        question: "Nová otázka",
        options: ["Možnost 1", "Možnost 2"],
        correctIndex: 0,
    });

    kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = quiz.value.questions.length - 1;
};

const deleteQuestion = (i: number) => {
    if (!quiz.value) return;
    if (i < 0 || i >= quiz.value.questions.length) return;

    quiz.value.questions.splice(i, 1);

    if (kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value >= quiz.value.questions.length) {
        kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = quiz.value.questions.length - 1;
    }
};

const dragFrom = ref<number | null>(null);
const dragTo = ref<number | null>(null);

const moveQuestion = (from: number, to: number) => {
    if (!quiz.value) return;
    if (from === to) return;

    const items = quiz.value.questions;
    const [moved] = items.splice(from, 1);
    
    if (!moved) return;
    
    items.splice(to, 0, moved);

    const active = kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value;

    if (active === from) {
        kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = to;
    } else if (from < active && active <= to) {
        kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value--;
    } else if (to <= active && active < from) {
        kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value++;
    }

    canSave.value = true;
};

const onDrop = () => {
    if (dragFrom.value !== null && dragTo.value !== null) {
        moveQuestion(dragFrom.value, dragTo.value);
    }

    dragFrom.value = null;
    dragTo.value = null;
};

onMounted(() => {
    window.addEventListener("beforeunload", (e) => {
        if (oldQuiz.value && JSON.stringify(oldQuiz.value) === JSON.stringify(quiz.value)) return;

        e.preventDefault();
        e.returnValue = "";
    });
});

</script>

<template>
    <Head>
        <Title>Úprava kvízu • Think different Academy</Title>
    </Head>

    <div :class="[$style.editMode, $style.quizContainer]" v-if="quiz">
        <ul :class="$style.questionProgress">
            <li
                v-for="(_, i) in quiz.questions"
                :key="i"
                draggable="true"
                :class="[
                    i === kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex && $style.active, 
                    i === dragFrom && $style.ghost
                ]"
                @dragstart="dragFrom = i"
                @dragover.prevent="dragTo = i"
                @drop.prevent="onDrop"
                @dragleave="dragTo = null"
                @dragend="() => {
                    dragFrom = null;
                    dragTo = null;
                }"
                @click="setQuestionIndex(i)"
            >
                <span :class="$style.dragHandle">⋮⋮</span>
                {{ i + 1 }}
            </li>

            <li :class="$style.add" @click="addQuestion"></li>
        </ul>
        <p
            :class="$style.editable"
            :contenteditable="true"
            @input="updateTitle(($event.target as HTMLElement).innerText)"
        >{{ quiz.title }}</p>
        <QuizQuestionCard
            v-if="quiz && currentQuestion && uuid"
            :question="currentQuestion"
            :edit-mode="true"
            @update:question="updateQuestion(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex, $event)"
            @deleteQuestion="deleteQuestion(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex)"
        />
        
<!--        <div :class="$style.controls">-->
<!--            <p @click="incrementQuestion(-1)">&lt;</p>-->
<!--            <p @click="incrementQuestion(1)">&gt;</p>-->
<!--        </div>-->
        
        <Button @click="saveQuiz" :disabled="!canSave">Uložit</Button>
    </div>
</template>

<style module lang="scss">
@use "../../../../../app" as *;

.editMode {
    .editable {
        @include editable;
    }
}

.quizContainer {
    position: relative;
    display: flex;
    flex-direction: column;
    gap: 16px;
    width: 500px;
    margin: auto;
    
    >p {
        font-size: 48px;
        font-weight: 600;
        margin: 0;
        text-align: center;
    }
    
    .controls {
        position: absolute;
        display: flex;
        justify-content: space-between;
        color: var(--text-color-secondary);
        user-select: none;
        padding: 0 16px;
        margin-top: 8px;
        width: 100%;
        top: 50%;
        transform: translateY(-50%);
        
        p {
            font-size: 48px;
            cursor: pointer;
            margin: 0;
        }
    }
    
    .questionProgress {
        list-style: none;
        display: flex;
        gap: 8px;
        padding: 0;
        margin: 0;
        justify-content: center;
        transition: all 0.3s;
        
        .draggableGroup {
            display: flex;
            gap: 8px;
        }
        
        li {
            width: 32px;
            height: 32px;
            border-radius: 50%;
            background-color: var(--background-color-secondary);
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: 600;
            color: var(--text-color-secondary);
            cursor: grab;
            transition: all 0.2s;
            user-select: none;
            
            &:active {
                cursor: grabbing;
            }
            
            .dragHandle {
                 font-size: 13px;
                 line-height: 1;
                 opacity: 0.5;
                 margin-right: 4px;
                 cursor: grab;
                 pointer-events: none;
             }
        }

        .ghost {
            opacity: 0.4;
        }

        .active {
            background-color: var(--accent-color-primary);
            color: white;
        }
        
        .add {
            font-size: 24px;
            font-weight: 600;
            position: relative;
            
            &:before {
                content: "";
                position: absolute;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                width: 16px;
                height: 16px;
                mask-image: url('/icons/plus.svg');
                mask-size: cover;
                mask-position: center;
                mask-repeat: no-repeat;
                background-color: var(--text-color-secondary);
            }
        }
    }
}
</style>