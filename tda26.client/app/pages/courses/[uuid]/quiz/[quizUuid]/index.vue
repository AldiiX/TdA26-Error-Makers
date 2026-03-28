<script setup lang="ts">
import { Head, Title } from '#components';
import type {Account, AnswerSubmission, Course, FeedPost, Question, Quiz} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import QuizQuestionCard from "~/components/pagespecific/QuizQuestionCard.vue";
import Modal from "~/components/Modal.vue";

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

                // pokud je finaltest, vyhod uzivatele
                if (quiz.mode === 'finaltest') {
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


const { uuid, quizUuid } = useRoute().params;

const quiz = useState<Quiz | null>(`quiz-${quizUuid}`);

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
    // console.log("totalTimeSeconds", totalTimeSeconds);

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
        .filter((entry): entry is AnswerSubmission | any => entry !== null);

    const response = await $fetch<{ resultUuid: string }>(getBaseUrl() + `/api/v1/courses/${uuid}/quizzes/${quizUuid}/submit`, {
        method: 'POST',
        body: {
            answers,
            totalTimeSeconds: totalTimeSeconds
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
};

let totalTimeSeconds = 0;

onMounted(() => {
    const interval = setInterval(() => {
        totalTimeSeconds++;
    }, 1000);

    onUnmounted(() => {
        clearInterval(interval);
    });
});

// ======== CHAT (pouze pro practice mode) ========
const isChatOpen = ref(false);
const chatPosts = ref<FeedPost[]>([]);
const chatMessage = ref("");
const isChatSending = ref(false);
let chatEventSource: EventSource | null = null;

const openChat = () => {
    isChatOpen.value = true;
};

const closeChat = () => {
    isChatOpen.value = false;
};

const sendChatMessage = async () => {
    if (!chatMessage.value.trim() || isChatSending.value) return;

    isChatSending.value = true;

    try {
        await $fetch<FeedPost>(`${getBaseUrl()}/api/v1/courses/${uuid}/feed`, {
            method: "POST",
            body: {
                message: chatMessage.value.trim(),
                type: "manual"
            }
        });
        chatMessage.value = "";
    } catch (e) {
        console.error("Failed to send chat message", e);
    } finally {
        isChatSending.value = false;
    }
};

const onChatKeydown = (e: KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        sendChatMessage();
    }
};

onMounted(async () => {
    if (!import.meta.client || quiz.value?.mode !== 'practice') return;

    // načtení existujících zpráv
    try {
        const posts = await $fetch<FeedPost[]>(`${getBaseUrl()}/api/v1/courses/${uuid}/feed`);
        chatPosts.value = posts.filter(p => p.type === 'manual').reverse();
    } catch (e) {
        console.error("Failed to load chat posts", e);
    }

    // SSE
    chatEventSource = new EventSource(`${getBaseUrl()}/api/v1/courses/${uuid}/feed/stream`, {
        withCredentials: true
    });

    chatEventSource.addEventListener("new_post", (event: MessageEvent) => {
        try {
            const post: FeedPost = JSON.parse(event.data);
            if (post.type === 'manual') {
                chatPosts.value = [...chatPosts.value, post];
            }
        } catch (e) {
            console.error("Failed to parse chat SSE event", e);
        }
    });

    chatEventSource.addEventListener("delete_post", (event: MessageEvent) => {
        try {
            const data: { uuid: string } = JSON.parse(event.data);
            chatPosts.value = chatPosts.value.filter(p => p.uuid !== data.uuid);
        } catch (e) {
            console.error("Failed to parse delete_post SSE event", e);
        }
    });

    chatEventSource.onerror = (err) => {
        console.error("Chat SSE error", err);
    };
});

onBeforeUnmount(() => {
    if (chatEventSource) {
        chatEventSource.close();
        chatEventSource = null;
    }
});
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

        <!-- Chat tlačítko – pouze pro practice mode -->
        <button
            v-if="quiz.mode === 'practice'"
            :class="$style.chatButton"
            type="button"
            title="Otevřít chat"
            @click="openChat"
        >
            💬
        </button>
    </div>

    <!-- Chat modal -->
    <Modal
        v-if="quiz?.mode === 'practice'"
        :enabled="isChatOpen"
        :can-be-closed-by-clicking-outside="true"
        :modal-style="{ maxWidth: '500px', height: '600px', display: 'flex', flexDirection: 'column' }"
        @close="closeChat"
    >
        <h3 style="margin: 0 0 16px;">Chat</h3>
        <div :class="$style.chatMessages">
            <div
                v-for="post in chatPosts"
                :key="post.uuid"
                :class="$style.chatMessage"
            >
                <strong>{{ post.author?.fullNameWithoutTitles ?? 'Uživatel' }}</strong>
                <p>{{ post.message }}</p>
                <span :class="$style.chatTime">{{ new Date(post.createdAt).toLocaleTimeString() }}</span>
            </div>
            <div v-if="chatPosts.length === 0" style="text-align: center; color: var(--text-color-secondary); padding: 16px;">
                Zatím žádné zprávy.
            </div>
        </div>
        <div :class="$style.chatInputRow">
            <textarea
                v-model="chatMessage"
                :class="$style.chatInput"
                placeholder="Napište zprávu..."
                rows="2"
                :disabled="isChatSending"
                @keydown="onChatKeydown"
            />
            <Button
                button-style="primary"
                :disabled="!chatMessage.trim() || isChatSending"
                @click="sendChatMessage"
            >Odeslat</Button>
        </div>
    </Modal>
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

.chatButton {
    position: fixed;
    bottom: 32px;
    right: 32px;
    width: 56px;
    height: 56px;
    border-radius: 50%;
    background-color: var(--accent-color-primary);
    color: white;
    font-size: 24px;
    border: none;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    transition: opacity 0.2s;
    z-index: 100;

    &:hover {
        opacity: 0.85;
    }
}

.chatMessages {
    flex: 1;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 12px;
    padding: 4px 0 12px;
    min-height: 0;
}

.chatMessage {
    background-color: var(--background-color-secondary);
    border-radius: 8px;
    padding: 10px 14px;

    strong {
        font-size: 13px;
        color: var(--accent-color-primary);
    }

    p {
        margin: 4px 0 0;
        font-size: 15px;
        word-break: break-word;
    }
}

.chatTime {
    font-size: 11px;
    color: var(--text-color-secondary);
}

.chatInputRow {
    display: flex;
    gap: 8px;
    align-items: flex-end;
    padding-top: 8px;
    border-top: 1px solid var(--border-color);
}

.chatInput {
    flex: 1;
    resize: none;
    border-radius: 8px;
    border: 1px solid var(--border-color);
    padding: 8px 12px;
    font-family: inherit;
    font-size: 14px;
    background-color: var(--background-color-secondary);
    color: var(--text-color-primary);

    &:focus {
        outline: none;
        border-color: var(--accent-color-primary);
    }

    &:disabled {
        opacity: 0.6;
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

    .chatButton {
        bottom: 16px;
        right: 16px;
    }
}
</style>
