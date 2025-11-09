<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch, computed } from "vue";
import type { Component } from "vue"; // typ pro vlastni komponentu

type Emits = {
    (e: "done"): void;
    (e: "progress", value: number): void;
};

const props = withDefaults(defineProps<{
    text: string;
    durationMs?: number;
    startDelayMs?: number;
    caret?: boolean;
    loop?: boolean;
    loopPauseMs?: number;
    ariaLabel?: string;
    // dovol string html tagu nebo vue komponentu
    element?: keyof HTMLElementTagNameMap | Component; // ts: jemnejsi typ
}>(), {
    durationMs: 2000,
    startDelayMs: 0,
    caret: true,
    loop: false,
    loopPauseMs: 800,
    element: "p"
});

const emit = defineEmits<Emits>();

const output = ref<string>("");
const isTyping = ref<boolean>(false);

let rafId = 0;
let startTs = 0;
let state: "idle" | "waiting" | "typing" | "loopPause" = "idle";

let segments: string[] = [];

/** rozseknu text na graphemy, abych nelamal znaky s diakritikou/emoji */
function toGraphemes(input: string): string[] {
    // pouziju locale-sensitive segmentaci, kdyz je k dispozici
    if (typeof (Intl as any)?.Segmenter === "function") {
        const seg = new (Intl as any).Segmenter(undefined, { granularity: "grapheme" });
        return Array.from(seg.segment(input), (s: any) => s.segment);
    }
    // fallback – array.from zvlada unicode lepe nez split("")
    return Array.from(input);
}

const totalChars = computed(() => segments.length);

/** hlavni animacni smycka pres rAF */
function frame(ts: number) {
    if (state === "waiting") {
        // cekam na start delay
        if (ts - startTs < props.startDelayMs) {
            rafId = requestAnimationFrame(frame);
            return;
        }
        state = "typing";
        startTs = ts;
        isTyping.value = true;
    }

    if (state === "typing") {
        const elapsed = ts - startTs;
        const progress = Math.min(1, props.durationMs > 0 ? elapsed / props.durationMs : 1);
        const count = Math.floor(progress * totalChars.value);
        output.value = segments.slice(0, count).join("");
        emit("progress", progress);

        if (progress < 1) {
            rafId = requestAnimationFrame(frame);
            return;
        }

        // hotovo jedno kolo
        output.value = segments.join("");
        isTyping.value = false;
        emit("done");

        if (props.loop) {
            state = "loopPause";
            startTs = ts;
            rafId = requestAnimationFrame(frame);
            return;
        }
        // konec bez smycky
        cancelAnimationFrame(rafId);
        return;
    }

    if (state === "loopPause") {
        if (ts - startTs >= props.loopPauseMs) {
            // restart smycky
            output.value = "";
            startTs = ts;
            state = "waiting";
        }
        rafId = requestAnimationFrame(frame);
    }
}

function start() {
    // priprava segmentu a start animace
    segments = toGraphemes(props.text);
    cancelAnimationFrame(rafId);
    startTs = performance.now();
    state = "waiting";
    rafId = requestAnimationFrame(frame);
}

onMounted(() => {
    // spustim jen na klientu
    start();
});

onBeforeUnmount(() => {
    // uklid – zrusim rAF
    if (rafId) { cancelAnimationFrame(rafId); }
});

/** pri zmene textu znovu odpocitam */
watch(() => props.text, () => start());
</script>

<template>
        <component :is="element">{{ output }}</component>
</template>

<style scoped>
.typewriter {
    /* nic specialniho; layout necham na kontextu */
}

/* jednoduchy blikajici kurzor */
.caret {
    display: inline-block;
    width: 1px;
    height: 1em;
    background: currentColor;
    transform: translateY(0.15em);
    animation: caret-blink 1s steps(1) infinite;
}

/* kdyz se pise, zvyraznim rychlejsim blikanim (volitelne) */
.caret.is-typing {
    animation-duration: .6s;
}

@keyframes caret-blink {
    50% { opacity: 0; }
}
</style>