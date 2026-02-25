<script setup lang="ts">
import type { QuizResultsSummary } from "#shared/types";
import Modal from "~/components/Modal.vue";
import VueApexCharts from "vue3-apexcharts";
import type { ApexOptions } from "apexcharts";
import { computed } from "vue";

/**
 * OPTIONAL navíc z parenta (abychom uměli ukázat i:
 * - úspěšnost jednotlivých kvízů (bar chart přes kurz)
 * - počet stažení materiálů
 * - počet otevření odkazů
 */
type CourseAnalyticsExtra = {
    perQuizSuccess?: Array<{ quizUuid: string; title: string; avgPercent: number; totalAttempts: number }>;
    materialDownloads?: number; // celkem za kurz
    linkOpens?: number;         // celkem za kurz
};

const props = defineProps<{
    enabled: boolean;
    quizResultsSummary: QuizResultsSummary | null;
    extra?: CourseAnalyticsExtra | null;
}>();

const emit = defineEmits<{ (e: "close"): void }>();

const summary = computed(() => props.quizResultsSummary);

// ---- default buckets (aby grafy nikdy nebyly "prázdné strukturou") ----
const defaultScoreBuckets = [
    "0–20%",
    "20–40%",
    "40–60%",
    "60–80%",
    "80–100%",
];

const defaultTimeBuckets = [
    "0–1 min",
    "1–3 min",
    "3–5 min",
    "5–10 min",
    "10+ min",
];

function normalizeDist(
    incoming: Array<{ label: string; count: number }> | undefined,
    defaults: string[]
) {
    const map = new Map((incoming ?? []).map(x => [x.label, Number(x.count) || 0]));
    return defaults.map(label => ({ label, count: map.get(label) ?? 0 }));
}

// ---- always full arrays (including zeros) ----
const scoreDist = computed(() =>
    normalizeDist((summary.value as any)?.scoreDistribution, defaultScoreBuckets)
);

const timeDist = computed(() =>
    normalizeDist((summary.value as any)?.timeDistribution, defaultTimeBuckets)
);

// ---- info cards ----
const totalAttempts = computed(() => summary.value?.totalAttempts ?? 0);
const averageScore = computed(() => summary.value?.averageScore ?? 0);
const averageTimeSpent = computed(() => summary.value?.averageTimeSpent ?? 0);
const averageScorePercentage = computed(() => summary.value?.averageScorePercentage ?? 0);

const mostCommonScoreBucket = computed(() => {
    const dist = scoreDist.value;
    if (!dist.length) return "—";
    const top = [...dist].sort((a, b) => b.count - a.count)[0];
    // když jsou všechno 0, je to stejně “—”, aby to nebylo matoucí
    if (!top || top.count === 0) return "—";
    return `${top.label} (${top.count})`;
});

const mostCommonTimeBucket = computed(() => {
    const dist = timeDist.value;
    if (!dist.length) return "—";
    const top = [...dist].sort((a, b) => b.count - a.count)[0];
    if (!top || top.count === 0) return "—";
    return `${top.label} (${top.count})`;
});

// ---- PIE (scoreDistribution) ----
const pieSeries = computed(() => scoreDist.value.map(x => x.count));
const pieOptions = computed<ApexOptions>(() => ({
    chart: { type: "pie" },
    labels: scoreDist.value.map(x => x.label),
    legend: { position: "top" },
    dataLabels: { enabled: true },
}));

// ---- 100% stacked bar (scoreDistribution) ----
const stackedSeries = computed(() =>
    scoreDist.value.map(d => ({ name: d.label, data: [d.count] }))
);

const stackedOptions = computed<ApexOptions>(() => ({
    chart: {
        type: "bar",
        stacked: true,
        stackType: "100%",
        toolbar: { show: false },
    },
    plotOptions: {
        bar: { horizontal: true, barHeight: "40%" },
    },
    xaxis: { categories: ["Score distribution"], labels: { show: false } },
    yaxis: { labels: { show: false } },
    legend: { position: "top" },
}));

// ---- LINE (timeDistribution) ----
const lineSeries = computed(() => [
    { name: "Time distribution", data: timeDist.value.map(x => x.count) },
]);

const lineOptions = computed<ApexOptions>(() => ({
    chart: { type: "line" },
    stroke: { curve: "smooth", width: 4 },
    xaxis: { categories: timeDist.value.map(x => x.label) },
    legend: { position: "top" },
}));

// ---- COURSE: per-quiz success (optional extra) ----
const perQuiz = computed(() => props.extra?.perQuizSuccess ?? []);
const perQuizSeries = computed(() => [{ name: "Úspěšnost (%)", data: perQuiz.value.map(x => x.avgPercent) }]);
const perQuizOptions = computed<ApexOptions>(() => ({
    chart: { type: "bar", toolbar: { show: false } },
    xaxis: { categories: perQuiz.value.map(x => x.title) },
    dataLabels: { enabled: true },
    plotOptions: { bar: { borderRadius: 6 } },
}));

const materialDownloads = computed(() => props.extra?.materialDownloads ?? null);
const linkOpens = computed(() => props.extra?.linkOpens ?? null);

// helper: “jsou vůbec nějaké pokusy?”
const hasAttempts = computed(() => (summary.value?.totalAttempts ?? 0) > 0);
</script>

<template>
    <Modal
        v-if="enabled"
        :enabled="enabled"
        can-be-closed-by-clicking-outside
        :modal-style="{ maxWidth: '1080px' }"
        @close="emit('close')"
    >
        <div v-if="!summary" style="padding: 16px;">
            Nejsou k dispozici data.
        </div>

        <div v-else :class="$style.quizResultsModal">
            <h3>Výsledky kvízu</h3>

            <div v-if="!hasAttempts" style="margin-bottom: 8px; opacity: 0.7;">
                Zatím nejsou žádné pokusy — grafy budou mít hodnoty 0.
            </div>

            <!-- INFO -->
            <div :class="$style.infoGrid">
                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Celkem pokusů</div>
                    <div :class="$style.infoValue">{{ totalAttempts }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Průměrné skóre</div>
                    <div :class="$style.infoValue">{{ averageScore }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Průměrná úspěšnost</div>
                    <div :class="$style.infoValue">{{ Math.round(averageScorePercentage) }}%</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Průměrný čas</div>
                    <div :class="$style.infoValue">{{ Math.round(averageTimeSpent) }} s</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Nejčastější skóre bucket</div>
                    <div :class="$style.infoValue">{{ mostCommonScoreBucket }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Nejčastější čas bucket</div>
                    <div :class="$style.infoValue">{{ mostCommonTimeBucket }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Počet stažení materiálů</div>
                    <div :class="$style.infoValue">{{ materialDownloads ?? "—" }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Počet otevření odkazů</div>
                    <div :class="$style.infoValue">{{ linkOpens ?? "—" }}</div>
                </div>
            </div>

            <!-- CHARTS -->
            <div :class="$style.chartsGrid">
                <div :class="$style.chartCard">
                    <h4>Time distribution</h4>
                    <VueApexCharts type="line" :series="lineSeries" :options="lineOptions" height="320" />
                </div>

                <div :class="$style.chartCard">
                    <h4>Score distribution (pie)</h4>
                    <VueApexCharts type="pie" :series="pieSeries" :options="pieOptions" height="320" />
                </div>

                <div :class="$style.chartCard">
                    <h4>Score distribution (100%)</h4>
                    <VueApexCharts type="bar" :series="stackedSeries" :options="stackedOptions" height="220" />
                </div>

                <div v-if="perQuiz.length" :class="$style.chartCard" style="grid-column: 1 / -1;">
                    <h4>Úspěšnost jednotlivých kvízů</h4>
                    <VueApexCharts type="bar" :series="perQuizSeries" :options="perQuizOptions" height="320" />
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
