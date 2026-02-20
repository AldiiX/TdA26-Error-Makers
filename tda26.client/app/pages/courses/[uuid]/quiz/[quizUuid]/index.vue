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
    headers: {
        'Cookie': useRequestHeaders(['cookie']).cookie || ''
    }
});

console.log("negr")

if (quizError.value) {
    console.error("Error fetching quiz:", quizError.value);
    await navigateTo(`/courses/${uuid}`);
}

const kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex = ref(0);

const currentQuestion = computed(() =>
    quiz.value?.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value]
);

const savedResponses = ref<AnswerSubmission[]>(
    Array(quiz.value?.questions.length || 0).fill(null)
);

const isGuid = (value?: string): value is string =>
    !!value && /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/.test(value);

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

const updateSelectedIndices = async (i: number, selectedIndices: number[]) => {
    if (!quiz.value) return;

    const question = quiz.value.questions[i];
    if (!question) return;

    if (!savedResponses.value[i]) savedResponses.value[i] = {} as AnswerSubmission;
    
    savedResponses.value[i] = {
        uuid: question.uuid!,
        selectedIndex: question.type === "singleChoice" ? selectedIndices[0] : undefined,
        selectedIndices: question.type === "multipleChoice" ? selectedIndices : undefined,
    };
};

const endQuiz = async () => {
    if (!quiz.value) return;

    const answers = quiz.value.questions
        .map((question, i) => {
            const saved = savedResponses.value[i];
            if (!saved || !isGuid(question.uuid)) return null;

            if (question.type === "singleChoice") {
                if (saved.selectedIndex === undefined) return null;
                return {
                    uuid: question.uuid,
                    selectedIndex: saved.selectedIndex
                } satisfies AnswerSubmission;
            }

            if (!saved.selectedIndices || saved.selectedIndices.length === 0) return null;
            return {
                uuid: question.uuid,
                selectedIndices: saved.selectedIndices
            } satisfies AnswerSubmission;
        })
        .filter((entry): entry is AnswerSubmission => entry !== null);

    const response = await $fetch<{ resultUuid: string }>(getBaseUrl() + `/api/v1/courses/${uuid}/quizzes/${quizUuid}/submit`, {
        method: 'POST',
        body: {
            answers,
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

    <div v-if="quiz" :class="[$style.quizContainer]">
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
            @update:selected-option="updateSelectedIndices(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex, $event)"
        />

        <div v-if="quiz.questions.length > 0" :class="$style.controls">
            <Button
                :disabled="kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex === 0"
                @click="incrementQuestionIndex(-1)"
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
@use "@/assets/variables" as app;

.editMode {
    .editable {
        @include app.editable;
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
        word-break: break-all;
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
        flex-wrap: wrap;

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

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
    .quizContainer {
        width: 100%;
        padding: 0 16px;
    }
    
    .controls {
        flex-direction: column;
    }
}
</style>
