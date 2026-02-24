<script setup lang="ts">
import type {QuizResultsSummary} from "#shared/types";
import Modal from "~/components/Modal.vue";
import VueApexCharts from "vue3-apexcharts";
import type {Course, Quiz} from "#shared/types";
import type {ApexOptions} from "apexcharts";
import { computed, onMounted, onBeforeUnmount, ref, watch } from "vue";


const quizResultsSummary = ref<QuizResultsSummary | null>(null);

type DistributionItem = {
    label: string;
    count: number;
};

const props = defineProps<{
    course?: Course,
    quiz?: Quiz,
    quizResultsSummary: QuizResultsSummary | null,
    enabled: boolean,
}>();

const emit = defineEmits<{
    (e: "close"): void;
}>();

watch(() => props.quizResultsSummary, (results) => {
    console.log("Received quiz results summary:", results);
});

const localSummary = ref<QuizResultsSummary | null>(null);

const summary = computed<QuizResultsSummary | null>(() => {
    return props.quizResultsSummary ?? localSummary.value ?? null;
});

async function fetchSummaryIfNeeded() {
    if (props.quizResultsSummary) return;
    if (!props.course?.uuid || !props.quiz?.uuid) return;

    try {
        const res = await fetch(`/api/v1/courses/${props.course.uuid}/quizzes/${props.quiz.uuid}/results-summary`);
        if (!res.ok) throw new Error("Failed to fetch quiz results summary");
        localSummary.value = await res.json();
    } catch (e) {
        console.error(e);
    }
}

onMounted(fetchSummaryIfNeeded);
watch(() => props.quizResultsSummary, () => {
    // když začneš posílat summary z parenta, nemusíme nic fetchovat
}, { deep: false });
// ---------- helpers ----------
function getScoreDist(): DistributionItem[] {
    const dist = (summary.value as any)?.scoreDistribution as DistributionItem[] | undefined;
    return (dist ?? []).filter((x) => x.count > 0);
}

function getTimeDist(): DistributionItem[] {
    const dist = (summary.value as any)?.timeDistribution as DistributionItem[] | undefined;
    return (dist ?? []).filter((x) => x.count > 0);
}

const totalAttempts = computed(() => {
    // fallback: sečtu scoreDistribution counts
    const s = getScoreDist();
    if (s.length) return s.reduce((a, b) => a + (Number(b.count) || 0), 0);
    const t = getTimeDist();
    if (t.length) return t.reduce((a, b) => a + (Number(b.count) || 0), 0);
    return 0;
});

const mostCommonScoreBucket = computed(() => {
    const s = getScoreDist();
    if (!s.length) return "—";
    const top = [...s].sort((a, b) => b.count - a.count)[0];
    return `${top?.label} (${top?.count})`;
});

const mostCommonTimeBucket = computed(() => {
    const t = getTimeDist();
    if (!t.length) return "—";
    const top = [...t].sort((a, b) => b.count - a.count)[0];
    return `${top?.label} (${top?.count})`;
});

// ---------- PIE (scoreDistribution) ----------
const pieSeries = computed<number[]>(() => getScoreDist().map((x) => x.count));
const pieOptions = computed<ApexOptions>(() => ({
    chart: { type: "pie" },
    labels: getScoreDist().map((x) => x.label),
    legend: { position: "top" },
    dataLabels: { enabled: true },
}));

// ---------- 100% STACKED BAR (scoreDistribution jako 1 bar rozdělený segmenty) ----------
const stackedCategories = computed(() => ["Score distribution"]);
const stackedSeries = computed(() => {
    const dist = getScoreDist();
    // každý bucket = vlastní série, data má jen jednu hodnotu => jeden bar
    return dist.map((d) => ({ name: d.label, data: [d.count] }));
});

const stackedOptions = computed<ApexOptions>(() => ({
    chart: {
        type: "bar",
        stacked: true,
        stackType: "100%",
        toolbar: { show: false },
    },
    plotOptions: {
        bar: {
            horizontal: true,
            barHeight: "40%",
            dataLabels: { position: "center" },
        },
    },
    xaxis: {
        categories: stackedCategories.value,
        labels: { show: false },
    },
    yaxis: { labels: { show: false } },
    legend: { position: "top" },
    dataLabels: {
        enabled: true,
        formatter: (val: number) => `${Math.round(val)}%`,
    },
    tooltip: {
        y: {
            formatter: (val: number) => `${val}`,
        },
    },
}));

// ---------- DYNAMIC UPDATING LINE ----------
const dynLabels = ref<string[]>([]);
const dynSeriesA = ref<number[]>([]);
const dynSeriesB = ref<number[]>([]);
const dynTimer = ref<number | null>(null);

function initDynamicSeries() {
    // vezmu jako základ timeDistribution (A) a scoreDistribution (B)
    const a = getTimeDist();
    const b = getScoreDist();

    // když nejsou data, udělám demo
    if (!a.length || !b.length) {
        dynLabels.value = Array.from({ length: 14 }, (_, i) => String(i + 1));
        dynSeriesA.value = [24, 33, 25, 21, 20, 6, 9, 15, 10, 50, 32, 12, 26, 33];
        dynSeriesB.value = [42, 13, 18, 28, 36, 35, 50, 32, 34, 20, 50, 36, 30, 32];
        return;
    }

    // sjednotím délku na min
    const n = Math.min(a.length, b.length);
    dynLabels.value = Array.from({ length: n }, (_, i) => String(i + 1));
    dynSeriesA.value = a.slice(0, n).map((x) => x.count);
    dynSeriesB.value = b.slice(0, n).map((x) => x.count);
}

function startDynamicUpdates() {
    stopDynamicUpdates();
    dynTimer.value = window.setInterval(() => {
        // posuň okno a přidej nový bod (random walk)
        const nextA = Math.max(0, Math.round((dynSeriesA.value.at(-1) ?? 10) + (Math.random() * 20 - 10)));
        const nextB = Math.max(0, Math.round((dynSeriesB.value.at(-1) ?? 10) + (Math.random() * 20 - 10)));

        if (dynSeriesA.value.length > 1) dynSeriesA.value = [...dynSeriesA.value.slice(1), nextA];
        if (dynSeriesB.value.length > 1) dynSeriesB.value = [...dynSeriesB.value.slice(1), nextB];
    }, 900);
}

function stopDynamicUpdates() {
    if (dynTimer.value) window.clearInterval(dynTimer.value);
    dynTimer.value = null;
}

watch(summary, () => {
    initDynamicSeries();
}, { immediate: true });

watch(() => props.enabled, (en) => {
    if (en) startDynamicUpdates();
    else stopDynamicUpdates();
}, { immediate: true });

onBeforeUnmount(stopDynamicUpdates);

const lineSeries = computed(() => [
    { name: "Time dist", data: dynSeriesA.value },
    { name: "Score dist", data: dynSeriesB.value },
]);

const lineOptions = computed<ApexOptions>(() => ({
    chart: { type: "line", animations: { enabled: true } },
    stroke: { curve: "smooth", width: 4 },
    xaxis: { categories: dynLabels.value },
    legend: { position: "top" },
    grid: { strokeDashArray: 4 },
}));

</script>

<template>
    <Modal
        v-if="enabled"
        :enabled="enabled"
        can-be-closed-by-clicking-outside
        :modal-style="{ maxWidth: '1080px' }"
        @close="emit('close')"
    >
        <div :class="$style.quizResultsModal">
            <h3>Výsledky kvízu</h3>

            <!-- INFO -->
            <div :class="$style.infoGrid">
                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Celkem pokusů</div>
                    <div :class="$style.infoValue">{{ totalAttempts }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Nejčastější skóre</div>
                    <div :class="$style.infoValue">{{ mostCommonScoreBucket }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Nejčastější čas</div>
                    <div :class="$style.infoValue">{{ mostCommonTimeBucket }}</div>
                </div>
            </div>

            <!-- CHARTS -->
            <div :class="$style.chartsGrid">
                <div :class="$style.chartCard">
                    <h4>Dynamic Updating Chart</h4>
                    <VueApexCharts type="line" :series="lineSeries" :options="lineOptions" height="320" />
                </div>

                <div :class="$style.chartCard">
                    <h4>Pie (score distribution)</h4>
                    <VueApexCharts type="pie" :series="pieSeries" :options="pieOptions" height="320" />
                </div>

                <div :class="$style.chartCard">
                    <h4>100% Stacked Bar (score distribution)</h4>
                    <VueApexCharts type="bar" :series="stackedSeries" :options="stackedOptions" height="220" />
                </div>
            </div>
        </div>
    </Modal>
</template>

<style module lang="scss">
.quizResultsModal {
    display: flex;
    flex-direction: column;
    gap: 16px;

    > h3 {
        margin: 0 0 8px;
        font-size: 22px;
    }
}

.infoGrid {
    display: grid;
    gap: 12px;
    grid-template-columns: repeat(3, minmax(0, 1fr));

    @media (max-width: 900px) {
        grid-template-columns: 1fr;
    }
}

.infoCard {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 10%, transparent 40%);
    border-radius: 12px;
    padding: 12px;
    background: var(--background-color-secondary, #fff);
}

.infoLabel {
    font-size: 12px;
    color: var(--text-color-secondary);
    margin-bottom: 6px;
}

.infoValue {
    font-size: 18px;
    font-weight: 700;
}

.chartsGrid {
    display: grid;
    gap: 16px;
    grid-template-columns: 1fr 1fr;

    @media (max-width: 900px) {
        grid-template-columns: 1fr;
    }
}

.chartCard {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 10%, transparent 40%);
    border-radius: 12px;
    padding: 12px;
    background: var(--background-color-secondary, #fff);

    h4 {
        margin: 0 0 10px;
        font-size: 16px;
    }
}
</style>
