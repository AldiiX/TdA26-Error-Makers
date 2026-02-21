<script setup lang="ts">
import { Head, Title } from '#components';
import type {Quiz, QuizResult} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import QuizQuestionCard from "~/components/pagespecific/QuizQuestionCard.vue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: [
        defineNuxtRouteMiddleware(async (to) => {
            const courseUuid = to.params.uuid as string;
            const quizUuid = to.params.quizUuid as string;
            const resultUuid = to.params.resultUuid as string;

            // pokud chybi uuid
            if (!quizUuid || !resultUuid) {
                return navigateTo(`/courses/${courseUuid}`);
            }

            try {
                const result = await $fetch<QuizResult>(`${getBaseUrl()}/api/v1/courses/${courseUuid}/quizzes/${quizUuid}/results/${resultUuid}`, {
                    query: { full: true },
                    headers: {
                        'Cookie': useRequestHeaders(['cookie']).cookie || ''
                    }
                });

                if (!result) {
                    return navigateTo(`/courses/${courseUuid}`);
                }
                const resultState = useState<QuizResult | null>(`quiz-result-${resultUuid}`, () => null);
                resultState.value = result;
            } catch (e) {
                console.error("Error loading result:", e);
                return navigateTo("/courses");
            }
        })
    ]
});

const { uuid, quizUuid, resultUuid } = useRoute().params;

const result = useState<QuizResult | null>(`quiz-result-${resultUuid}`);

const kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex = ref(0);

const currentQuestion = computed(() =>
    result.value?.quiz.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value]
);

const incrementQuestionIndex = (i: number) => {
    if (!result.value) return;

    const newIndex = kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value + i;
    if (newIndex < 0) return;
    if (newIndex >= result.value.quiz.questions.length) {
        return;
    }

    kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = newIndex;
};

const incrementQuestion = (i: number) => {
    if (!result.value) return;

    const newIndex = kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value + i;

    if (newIndex < 0) return;
    if (newIndex >= result.value.quiz.questions.length) return;

    kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value = newIndex;
};

const setQuestionIndex = (i: number) => {
    if (!result.value) return;
    if (i < 0 || i >= result.value.quiz.questions.length) return;

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
        <Title>Výsledek kvízu • Think different Academy</Title>
    </Head>

    <div v-if="result" :class="$style.container">
        <div :class="$style.scoreSection">
            <h1>Výsledek kvízu {{ result.quiz.title }}</h1>
            <p>Skóre: {{ result.score }} / {{ result.quiz.questions.length }} ({{ ((result.score / result.quiz.questions.length) * 100).toFixed(0) }}%)</p>
        </div>
        <ul :class="$style.questionProgress">
            <li
                v-for="(q, i) in result.quiz.questions"
                :key="i"
                :class="[
                    i === kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex && $style.active,
                    q.isCorrect ? $style.correct : $style.incorrect
                    ]"
                @click="setQuestionIndex(i)"
            >{{ i + 1 }}</li>
        </ul>
        <QuizQuestionCard
            v-if="result && currentQuestion && uuid"
            :question="currentQuestion"
            mode="result"
            :selected-option="result.quiz.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex]?.selectedIndices ?? []"
        />

        <div :class="$style.controls">
            <Button
                :disabled="kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex === 0"
                @click="incrementQuestionIndex(-1)"
            >Předchozí</Button>
            <Button
                :disabled="kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex === result.quiz.questions.length - 1"
                @click="incrementQuestionIndex(1)"
            >Další</Button>
        </div>
    </div>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.container {
    display: flex;
    flex-direction: column;
    gap: 16px;
    width: 500px;
    margin: auto;
    position: relative;
    
    .scoreSection {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        margin-bottom: 12px;
        
        h1 {
            font-size: 48px;
            margin: 0;
            text-align: center;
            word-break: break-all;
        }
        
        p {
            font-size: 20px;
            margin: 0;
        }
    }

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

            &.correct {
                border: 2px solid var(--color-success);
            }

            &.incorrect {
                border: 2px solid var(--color-error);
            }
        }

        .active {
            background-color: var(--accent-color-primary) !important;
            color: white;
        }
    }
}

@media screen and (max-width: 760px) {
    .container {
        width: 100%;
        padding: 0 16px;
    }

    .controls {
        flex-direction: column;
    }
}
</style>