<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount } from "vue";

/** props */
type Props = {
    /** cilova hodnota */
    value: number;
    /** delka animace v ms */
    durationMs?: number;
    /** pocet desetinnych mist (zaokrouhleno) */
    decimals?: number;
    /** locale pro formatovani (napr. "cs-CZ") */
    locale?: string;
    /** volitelne options pro Intl.NumberFormat (napr. { style: "currency", currency: "CZK" }) */
    formatOptions?: Intl.NumberFormatOptions;
    /** text pred cislem */
    prefix?: string;
    /** text za cislem */
    suffix?: string;
    /** typ easing funkce */
    easing?: "linear" | "easeOutCubic";
    /** startovni hodnota, ze ktere se bude pripocitavat */
    start?: number;
    numberClass?: string;
};

const props = withDefaults(defineProps<Props>(), {
    durationMs: 1200,
    decimals: 0,
    easing: "easeOutCubic",
    start: 0
});

/** internal state */
const displayValue = ref(props.start);
let rafId = 0;
let startTime: number | null = null;
let startValue = props.start;
let targetValue = props.value;

/** easing; ease-out-cubic pro prijemny dobeh (viz bezne easing tabulky) */
// comments: funkce vraci prubeh v intervalu 0..1
function ease(t: number): number {
    if (props.easing === "linear") { return t; }
    t -= 1;
    return t * t * t + 1; // ease-out-cubic
}

function frame(now: number) {
    if (startTime === null) { startTime = now; }
    const elapsed = now - startTime;
    const t = Math.min(1, elapsed / props.durationMs);
    const e = ease(t);
    displayValue.value = startValue + (targetValue - startValue) * e;

    if (t < 1) { rafId = requestAnimationFrame(frame); }
}

function startAnimation(from: number, to: number) {
    cancelAnimationFrame(rafId);
    startTime = null;
    startValue = from;
    targetValue = to;
    rafId = requestAnimationFrame(frame);
}

/** re-run pri zmene cilove hodnoty */
watch(() => props.value, (next) => {
    startAnimation(displayValue.value, next);
});

/** spustit po mountu */
onMounted(() => {
    startAnimation(props.start, props.value);
});

onBeforeUnmount(() => { cancelAnimationFrame(rafId); });

/** zaokrouhleni bez alokaci */
const rounded = computed(() => {
    const factor = Math.pow(10, props.decimals);
    return Math.round(displayValue.value * factor) / factor;
});

/** volitelne lokalizovane formatovani pres Intl.NumberFormat */
const formatted = computed(() => {
    if (props.locale || props.formatOptions) {
        const fmt = new Intl.NumberFormat(props.locale, props.formatOptions);
        return fmt.format(rounded.value);
    }
    return rounded.value.toFixed(props.decimals);
});
</script>

<template>
  <div :class="$style.counter">
    <p :class="[$style.number, props.numberClass]">
      {{ props.prefix }}{{ formatted }}{{ props.suffix }}
    </p>
  </div>
</template>

<style module lang="scss">
.counter {
    display: inline-block;
    will-change: contents;
}

.number {
    display: inline-block;
    font-variant-numeric: tabular-nums;
    transition: filter 0.2s ease, transform 0.2s ease;
}
</style>