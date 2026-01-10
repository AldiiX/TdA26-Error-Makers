<script setup lang="ts">
import { Head, Title } from '#components';
import type {AnswerSubmission, Course, Question, Quiz} from "#shared/types";
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
    await navigateTo(`/courses/${uuid}`);
}

const kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex = ref(0);

const currentQuestion = computed(() =>
    quiz.value?.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value]
);

const savedResponses = ref<AnswerSubmission[]>([]);

const incrementQuestionIndex = (i: number) => {
    if (!quiz.value) return;
    
    const newIndex = kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value + i;
    if (newIndex < 0) return;
    if (newIndex >= quiz.value.questions.length) {
        endQuiz();
        return;
    }
    
    kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = newIndex;
};

const updateSelectedIndices = (i: number, selectedIndices: number[]) => {
    if (!quiz.value) return;

    const question = quiz.value.questions[i];
    if (!question) return;

    savedResponses.value[i] = {
        uuid: question.uuid!,
        selectedIndex: question.type === "singleChoice" ? selectedIndices[0] : undefined,
        selectedIndices: question.type === "multipleChoice" ? selectedIndices : undefined,
    };
};

const endQuiz = async () => {
    const response = await $fetch<{ resultUuid: string }>(getBaseUrl() + `/api/v2/courses/${uuid}/quizzes/${quizUuid}/submit`, {
        method: 'POST',
        body: {
            answers: savedResponses.value,
        }
    });
    
    window.location.href = `/courses/${uuid}/quiz/${quizUuid}/result/${response.resultUuid}`;
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

    // console.log(savedResponses.value)
    // console.log(
    //     savedResponses.value[
    //         kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value
    //         ]?.selectedIndex !== undefined
    //         ? [
    //             savedResponses.value[
    //                 kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value
    //                 ]!.selectedIndex!
    //         ]
    //         : savedResponses.value[
    //         kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value
    //         ]?.selectedIndices ?? [])
};

</script>

<template>
    <Head>
        <Title>Kvíz • Think different Academy</Title>
    </Head>

    <div :class="[$style.quizContainer]" v-if="quiz">
        <ul :class="$style.questionProgress">
            <li 
                v-for="(_, i) in quiz.questions"
                :key="i"
                :class="{ [$style.active]: i === kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex }"
                @click="setQuestionIndex(i)"
            >{{ i + 1 }}</li>
        </ul>
        <p>{{ quiz.title }}</p>
        <QuizQuestionCard
            v-if="quiz && currentQuestion && uuid && quiz.questions.length > 0"
            :question="currentQuestion"
            @update:selectedOption="updateSelectedIndices(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex, $event)"
            :selected-option="
              savedResponses[
                kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex
              ]?.selectedIndex !== undefined
                ? [
                    savedResponses[
                      kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex
                    ]!.selectedIndex!
                  ]
                : savedResponses[
                    kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex
                  ]?.selectedIndices ?? []
            "
        />

        <div :class="$style.controls" v-if="quiz.questions.length > 0">
            <Button
                @click="incrementQuestionIndex(-1)"
                :disabled="kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex === 0"
            >Předchozí</Button>
            <Button
                @click="incrementQuestionIndex(1)"
            >{{ kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex === quiz.questions.length - 1 ? "Dokončit" : "Další" }}</Button>
        </div>
        
        <div>
            <p v-if="quiz.questions.length === 0" style="text-align: center;">
                Tento kvíz neobsahuje žádné otázky.
            </p>
        </div>
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
    position: relative;

    >p {
        font-size: 48px;
        font-weight: 600;
        margin: 0;
        text-align: center;
    }

    .controls {
        display: flex;
        justify-content: space-between;
        gap: 16px;

        button {
            width: 100%;
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
            cursor: pointer;
            transition: all 0.2s;
            user-select: none;
        }
        
        .active {
            background-color: var(--accent-color-primary);
            color: white;
        }
    }
}
</style>