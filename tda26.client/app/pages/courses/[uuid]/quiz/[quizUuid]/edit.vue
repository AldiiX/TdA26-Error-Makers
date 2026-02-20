<script setup lang="ts">
import { Head, Title } from '#components';
import type {Account, Course, Question, Quiz} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import QuizQuestionCard from "~/components/pagespecific/QuizQuestionCard.vue";
import { push } from "notivue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: [
        defineNuxtRouteMiddleware(async (to) => {
            const courseUuid = to.params.uuid as string;
            const quizUuid = to.params.quizUuid as string;

            // pokud chybi uuid
            if (!quizUuid) {
                return navigateTo(`/courses/${courseUuid}`);
            }

            try {
                const quiz = await $fetch<Quiz>(`${getBaseUrl()}/api/v1/courses/${courseUuid}/quizzes/${quizUuid}`, {
                    query: { full: true },
                    headers: {
                        'Cookie': useRequestHeaders(['cookie']).cookie || ''
                    }
                });

                if (!quiz) {
                    return navigateTo(`/courses/${courseUuid}`);
                }
                const quizState = useState<Quiz | null>(`quiz-${quizUuid}`, () => null);
                quizState.value = quiz;
            } catch (e) {
                    console.error("Error loading quiz:", e);
                return navigateTo("/courses");
            }
        })
    ]
});

const loggedAccount = useState<Account | null>('loggedAccount');
const { uuid, quizUuid } = useRoute().params;

const isActionInProgress = ref(false);

const quiz = useState<Quiz | null>(`quiz-${quizUuid}`);

const { data: courseSmall } = await useFetch<Course>(`${getBaseUrl()}/api/v1/courses/${uuid}`, {
    query: { full: false },
    server: true,
    key: `course-${uuid}-small`,
});

if (loggedAccount.value?.type !== 'admin' && (!loggedAccount.value || loggedAccount.value.uuid !== courseSmall.value?.account?.uuid)) {
    // pokud neni vlastnik kurzu, nema pravo editovat
    await navigateTo(`/courses/${uuid}/quiz/${quizUuid}`);
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

const recheckQuiz = () => {
    if (!quiz.value) {
        canSave.value = false;
        return;
    }

    if (!quiz.value.title?.trim()) {
        canSave.value = false;
        return;
    }
    if (quiz.value.questions.length === 0) {
        canSave.value = false;
        return;
    }

    canSave.value = quiz.value.questions.every(isQuestionValid);
}

const canSave = ref(false);

const isQuestionValid = (q: Question): boolean => {
    if (!q.question?.trim()) return false;
    if (!q.options || q.options.length < 2) return false;
    if (q.options.some(o => !o.trim())) return false;
    
    if (q.correctIndex === undefined) {
        return (
            Array.isArray(q.correctIndices) &&
            q.correctIndices.length > 0 &&
            q.correctIndices.every(
                i => i >= 0 && i < q.options.length
            )
        );
    } else {
        return (
            q.correctIndex >= 0 &&
            q.correctIndex < q.options.length
        );
    }
};

const saveQuiz = async () => {
    if (!quiz.value || !canSave.value) return;
    
    isActionInProgress.value = true;
    
    const dto = {
        uuid: quiz.value!.uuid.startsWith("new-") ? undefined : quiz.value!.uuid,
        title: quiz.value!.title,
        attemptsCount: quiz.value!.attemptsCount,
        questions: quiz.value!.questions.map((q, index) => ({
            ...mapQuestionToDto(q),
            order: index,
        })),
    };
    
    dto.questions.every((q) => {
        if (q.uuid && q.uuid.startsWith("new-")) {
            delete q.uuid;
        }
        
        return true;
    });

    await $fetch(getBaseUrl() + `/api/v1/courses/${uuid}/quizzes/${quizUuid}`, {
        method: 'PUT',
        body: dto,
    }).finally(() => {
        isActionInProgress.value = false;
    });
    
    oldQuiz.value = JSON.parse(JSON.stringify(quiz.value));
    push.success({
        title: "Kvíz uložen",
        message: "Kvíz byl úspěšně uložen.",
        duration: 4000
    });
};

const saveAndExit = async () => {
    await saveQuiz();
    window.location.href = (`/courses/${uuid}/quiz/${quizUuid}`);
};

const updateQuestion = (i: number, patch: Partial<Question>) => {    
    if (!quiz.value) return;

    quiz.value.questions.splice(i, 1, {
        ...quiz.value.questions[i],
        ...patch,
    } as Question);

    const q = quiz.value.questions[i];
    if (!q) return;
    
    recheckQuiz();
};

// Helper functions to preserve cursor position
const saveCursorPosition = (element: HTMLElement) => {
    const selection = window.getSelection();
    if (!selection || selection.rangeCount === 0) return null;
    
    const range = selection.getRangeAt(0);
    const preSelectionRange = range.cloneRange();
    preSelectionRange.selectNodeContents(element);
    preSelectionRange.setEnd(range.startContainer, range.startOffset);
    
    return preSelectionRange.toString().length;
};

const restoreCursorPosition = (element: HTMLElement, position: number) => {
    const selection = window.getSelection();
    if (!selection) return;
    
    const createRange = (node: Node, chars: { count: number }): Range | null => {
        if (chars.count === 0) {
            const range = document.createRange();
            range.setStart(node, 0);
            range.setEnd(node, 0);
            return range;
        }
        
        if (node.nodeType === Node.TEXT_NODE) {
            const textNode = node as Text;
            if (textNode.length >= chars.count) {
                const range = document.createRange();
                range.setStart(textNode, chars.count);
                range.setEnd(textNode, chars.count);
                return range;
            } else {
                chars.count -= textNode.length;
            }
        } else {
            for (let i = 0; i < node.childNodes.length; i++) {
                const range = createRange(node.childNodes[i]!, chars);
                if (range) return range;
            }
        }
        
        return null;
    };
    
    const range = createRange(element, { count: position });
    if (range) {
        selection.removeAllRanges();
        selection.addRange(range);
    }
};

const updateTitle = (e: Event) => {
    if (!quiz.value) return;
    
    const element = e.target as HTMLElement;
    const newTitle = element.innerText;
    const cursorPos = saveCursorPosition(element);
    
    quiz.value.title = newTitle;
    
    // Restore cursor position after Vue updates the DOM
    nextTick(() => {
        if (cursorPos !== null) {
            restoreCursorPosition(element, cursorPos);
        }
    });
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
        uuid: "new-" + crypto.randomUUID(),
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

    recheckQuiz();
    
    questionRenderRemountKey.value++;
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

    recheckQuiz();
};

const onDrop = () => {
    if (dragFrom.value !== null && dragTo.value !== null) {
        moveQuestion(dragFrom.value, dragTo.value);
    }

    dragFrom.value = null;
    dragTo.value = null;
};

onMounted(() => {
    // Warn user about unsaved changes
    if (!import.meta.dev) {
        window.addEventListener("beforeunload", (e) => {
            if (oldQuiz.value && JSON.stringify(oldQuiz.value) === JSON.stringify(quiz.value)) return;

            e.preventDefault();
            e.returnValue = "";
        });
    }
});

const addQuestionOption = (i: number) => {
    if (!quiz.value) return;
    
    const question = quiz.value.questions[i];
    if (!question) return;
    
    question.options.push(`Možnost ${question.options.length + 1}`);
    
    updateQuestion(i, { options: question.options });
    
    questionRenderRemountKey.value++;
};

const questionRenderRemountKey = ref(0);

const questionRenderKey = computed(() => {
    const q = quiz.value?.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value];
    if (!q) return 0;
    
    const index = kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex.value;

    return `${index}-${q.uuid ?? 'new'}-${questionRenderRemountKey.value}`;
});

const removeQuestionOption = (questionIndex: number, optionIndex: number) => {
    if (!quiz.value) return;

    const question = quiz.value.questions[questionIndex];
    if (!question) return;

    question.options.splice(optionIndex, 1);

    updateQuestion(questionIndex, { options: question.options });

    questionRenderRemountKey.value++;
};

</script>

<template>
    <Head>
        <Title>Úprava kvízu • Think different Academy</Title>
    </Head>

    <div v-if="quiz" :class="[$style.editMode, $style.quizContainer]">
        <ul :class="$style.questionProgress">
            <li
                v-for="(q, i) in quiz.questions"
                :key="q.uuid ?? i"
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

            <li :class="$style.add" @click="addQuestion"/>
        </ul>
        <p
            :class="$style.editable"
            :contenteditable="true"
            @input="updateTitle($event)"
        >{{ quiz.title }}</p>
        <QuizQuestionCard
            v-if="quiz && quiz.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex] && uuid"
            :key="questionRenderKey"
            :question="quiz.questions[kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex]!"
            mode="edit"
            @update:question="updateQuestion(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex, $event)"
            @delete-question="deleteQuestion(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex)"
            @add-question-option="addQuestionOption(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex)"
            @remove-question-option="removeQuestionOption(kvizovyIndexNaJednotlivyKvizProKvizVyuzitiProReferencniIntegrituAbyKvizZobrazeniMelJednuOtazkuSamenSamenIndexSamenAstarSeranVasMaMocRadIndexIndex, $event)"
        />
        
<!--        <div :class="$style.controls">-->
<!--            <p @click="incrementQuestion(-1)">&lt;</p>-->
<!--            <p @click="incrementQuestion(1)">&gt;</p>-->
<!--        </div>-->
        
        <div :class="$style.buttonGroup">
            <Button :disabled="!canSave || isActionInProgress" button-style="secondary" @click="saveQuiz">Uložit</Button>
            <Button :disabled="!canSave || isActionInProgress" button-style="primary" @click="saveAndExit">Uložit a odejít</Button>
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
    position: relative;
    display: flex;
    flex-direction: column;
    gap: 16px;
    width: 600px;
    margin: auto;
    
    >p {
        font-size: 48px;
        font-weight: 600;
        margin: 0;
        text-align: center;
        word-break: break-all;
    }
    
    .buttonGroup {
        display: flex;
        gap: 12px;
        justify-content: center;

        button {
            width: 100%;
        }
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
        flex-wrap: wrap;
        
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
                cursor: pointer;
            }
        }
    }
}

@media screen and (max-width: 760px) {
    .quizContainer {
        width: 100%;
        padding: 0 16px;
    }

    .buttonGroup {
        flex-direction: column;
    }
}
</style>