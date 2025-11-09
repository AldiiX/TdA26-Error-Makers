<script setup lang="ts">
import {ref, computed, onMounted, onBeforeUnmount, watch, type Component} from "vue";

type Emits = {
    (e: "complete"): void;
};

const emit = defineEmits<Emits>();

const props = withDefaults(defineProps<{
    text: string;
    /** slova vs. pismena */
    animateBy?: "words" | "letters";
    /** smer pohybu behem doostreni */
    direction?: "top" | "bottom";
    /** zpozdeni mezi segmenty v ms */
    delay?: number;
    /** delka animace jednoho segmentu v ms */
    durationMs?: number;
    /** css easing */
    easing?: string;
    /** kdy spustit podle pruniku s viewportem */
    threshold?: number;
    /** root margin pro intersection observer */
    rootMargin?: string;
    /** po prvnim spusteni prestat pozorovat */
    once?: boolean;
    /** root element tag/komponenta */
    tag?: keyof HTMLElementTagNameMap | Component;
    /** aria popis (fallback je samotny text) */
    ariaLabel?: string;
}>(), {
    animateBy: "words",
    direction: "top",
    delay: 150,
    durationMs: 700,
    easing: "cubic-bezier(0.4, 0, 0.2, 1)",
    threshold: 0.1,
    rootMargin: "0px",
    once: true,
    tag: "p"
});

const rootRef = ref<HTMLElement | null>(null);
const inView = ref(false);

// priprava segmentu
function toGraphemes(input: string): string[] {
    // pouziju segmenter kdyz je dostupny, jinak fallback
    if (typeof (Intl as any)?.Segmenter === "function") {
        const seg = new (Intl as any).Segmenter(undefined, {granularity: "grapheme"});
        return Array.from(seg.segment(input), (s: any) => s.segment);
    }
    return Array.from(input);
}

type Token = { text: string; space: boolean };
const tokens = computed<Token[]>(() => {
    if (props.animateBy === "words") {
        // rozdelim na slova a mezery (mezery nerenderuji jako animovane spany)
        const parts = props.text.split(/(\s+)/);
        return parts.map(p => ({text: p, space: /\s+/.test(p)}));
    } else {
        return toGraphemes(props.text).map(ch => ({text: ch, space: false}));
    }
});

// indexy jen animovanych segmentu
const animatedIndices = computed(() => tokens.value
    .map((t, i) => ({i, space: t.space}))
    .filter(t => ! t.space)
    .map(t => t.i)
);

const lastAnimatedIndex = computed(() => animatedIndices.value.at(-1) ?? -1);

// observer
let io: IntersectionObserver | null = null;

function mountObserver() {
    if (! import.meta.client || ! rootRef.value) {
        return;
    }
    io = new IntersectionObserver(([entry]) => {
        if (entry.isIntersecting) {
            inView.value = true;
            if (props.once && io && rootRef.value) {
                io.unobserve(rootRef.value);
            }
        }
    }, {threshold: props.threshold, rootMargin: props.rootMargin});
    io.observe(rootRef.value);
}

onMounted(() => {
    mountObserver();
});
onBeforeUnmount(() => {
    if (io) {
        io.disconnect();
        io = null;
    }
});

// znovunacteni pri zmene textu (pokud chceme animaci opet)
watch(() => props.text, () => {
    inView.value = false;
    // znovu zacit pozorovat
    if (io && rootRef.value) {
        io.observe(rootRef.value);
    }
});
</script>

<template>
    <component
            :is="props.tag"
            ref="rootRef"
            class="blur-text flex flex-wrap"
            :class="[{ 'in-view': inView }, props.direction === 'top' ? 'dir-top' : 'dir-bottom']"
            :style="{
      '--segment-duration': props.durationMs + 'ms',
      '--segment-ease': props.easing
    } as any"
            :aria-label="props.ariaLabel ?? props.text"
    >
        <template v-for="(t, i) in tokens" :key="i">
            <!-- neanimovane mezery -->
            <span v-if="t.space" aria-hidden="true">{{ t.text }}</span>

            <!-- animovany segment -->
            <span
                    v-else
                    class="segment"
                    :style="{ '--segment-delay': (animatedIndices.indexOf(i) * props.delay) + 'ms' } as any"
                    :data-last="i === lastAnimatedIndex"
                    @animationend="(e) => { if ((e.target as HTMLElement).dataset.last === 'true') emit('complete'); }"
            >
        {{ t.text }}
      </span>
        </template>
    </component>
</template>

<style scoped>
/* zakladni vzhled segmentu */
.segment {
    display: inline-block;
    will-change: transform, filter, opacity; /* hint pro vykreslovani */
}

/* hodnoty podle smeru (mensi 'nadzvih' uprostred stejne jako shadcn ukazka) */
.dir-top {
    --y-from: -50px;
    --y-mid: 5px;
}

.dir-bottom {
    --y-from: 50px;
    --y-mid: -5px;
}

/* aktivace animace po vstupu do viewportu */
.in-view .segment {
    animation-name: blur-reveal;
    animation-duration: var(--segment-duration, 700ms);
    animation-timing-function: var(--segment-ease, cubic-bezier(0.4, 0, 0.2, 1));
    animation-fill-mode: both;
    animation-delay: var(--segment-delay, 0ms);
}

/* klicove snimky: rozostreno -> polodoostreno -> ostre */
@keyframes blur-reveal {
    0% {
        filter: blur(10px);
        opacity: 0;
        transform: translateY(var(--y-from));
    }
    60% {
        filter: blur(5px);
        opacity: 0.5;
        transform: translateY(var(--y-mid));
    }
    100% {
        filter: blur(0px);
        opacity: 1;
        transform: translateY(0);
    }
}
</style>
