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
        questions: quiz.value!.questions.map(mapQuestionToDto),
    };

    await $fetch(getBaseUrl() + `/api/v1/courses/${uuid}/quizzes/${quizUuid}`, {
        method: 'PUT',
        body: dto,
    });
};

const updateQuestion = (i: number, patch: Partial<Question>) => {
    // TODO: when updating question it sets it to the back. instead keep the position
    
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
    if (quiz.value) {
        quiz.value.title = newTitle;
    }
};

</script>

<template>
    <Head>
        <Title>Úprava kvízu • Think different Academy</Title>
    </Head>

    <div :class="[$style.editMode, $style.quizContainer]" v-if="quiz">
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
        />
        
        <Button @click="saveQuiz" :disabled="!canSave">Save</Button>
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
}
</style>