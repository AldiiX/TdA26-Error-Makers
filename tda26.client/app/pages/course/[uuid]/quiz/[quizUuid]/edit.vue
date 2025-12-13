<script setup lang="ts">
import { Head, Title } from '#components';
import type {Course, Quiz} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";

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

const selectedIndices = ref<number[]>([]);

const selectOption = (index: number) => {
    if (!quiz.value) return;
    const currentType = quiz.value.questions?.[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value]?.type;
    
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
};

</script>

<template>
    <Head>
        <Title>Úprava kvízu • Think different Academy</Title>
    </Head>

    <div :class="$style.quizContainer" v-if="quiz">
        <p>{{ quiz.title }}</p>
        <div :class="$style.questionContainer">
            <div :class="$style.title">
                <p>{{ quiz.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex]?.question }}</p>
            </div>
            <ul>
                <li v-for="(option, index) in quiz.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex]?.options" :key="index">
                    <Button 
                        :button-style="selectedIndices.includes(index) ? 'primary' : 'tertiary'"
                        @click="selectOption(index)"
                    >{{ option }}</Button>
                </li>
            </ul>
        </div>
    </div>
</template>

<style module lang="scss">
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
                    border: none;
                }
            }
        }
    }
}
</style>