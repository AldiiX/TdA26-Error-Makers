<script setup lang="ts">
import type { QuizResultsSummary } from "#shared/types";
import Modal from "~/components/Modal.vue";
import VueApexCharts from "vue3-apexcharts";
import type { ApexOptions } from "apexcharts";
import { computed } from "vue";

const props = defineProps<{
    enabled: boolean;
    quizResultsSummary: QuizResultsSummary | null;
}>();

const emit = defineEmits<{ (e: "close"): void }>();

const summary = computed(() => props.quizResultsSummary);

// Globální stav: kvíz nemá žádné informace
const hasQuizInfo = computed(() => !!summary.value && (summary.value.totalAttempts ?? 0) > 0);

// --- jednoduché formátování (nepřidává data, jen prezentace) ---
function round2(n: number) {
    return Math.round(n * 100) / 100;
}
function formatSeconds(sec: number) {
    const s = Math.max(0, Math.round(sec));
    if (s < 60) return `${s} s`;
    const m = Math.floor(s / 60);
    const r = s % 60;
    return `${m} min ${r} s`;
}

// INFO
const totalAttempts = computed(() => summary.value?.totalAttempts ?? 0);
const averageScore = computed(() => summary.value?.averageScore ?? 0);
const averageScorePercentage = computed(() => summary.value?.averageScorePercentage ?? 0);
const averageTimeSpent = computed(() => summary.value?.averageTimeSpent ?? 0);

// --- distributions: pouze z API, žádné default buckety ---
const scoreDist = computed(() => summary.value?.scoreDistribution ?? []);
const timeDist = computed(() => summary.value?.timeDistribution ?? []);

// součty bucketů (pro render rozhodnutí + diagnostiku)
const scoreBucketsTotal = computed(() => scoreDist.value.reduce((a, b) => a + (Number(b.count) || 0), 0));
const timeBucketsTotal = computed(() => timeDist.value.reduce((a, b) => a + (Number(b.count) || 0), 0));

const canRenderScoreCharts = computed(() => scoreBucketsTotal.value > 0);
const canRenderTimeChart = computed(() => timeBucketsTotal.value > 0);

// nejčastější bucket
const mostCommonScoreBucket = computed(() => {
    const dist = scoreDist.value;
    if (!dist.length) return "—";
    const top = [...dist].sort((a, b) => (Number(b.count) || 0) - (Number(a.count) || 0))[0];
    if (!top || (Number(top.count) || 0) === 0) return "—";
    return `${top.label} (${top.count})`;
});

const mostCommonTimeBucket = computed(() => {
    const dist = timeDist.value;
    if (!dist.length) return "—";
    const top = [...dist].sort((a, b) => (Number(b.count) || 0) - (Number(a.count) || 0))[0];
    if (!top || (Number(top.count) || 0) === 0) return "—";
    return `${top.label} (${top.count})`;
});

// PIE (skóre) – ApexCharts pie potřebuje number[] + labels[] citeturn0search12turn0search4
const pieSeries = computed<number[]>(() => scoreDist.value.map(x => Number(x.count) || 0));
const pieOptions = computed<ApexOptions>(() => ({
    labels: scoreDist.value.map(x => x.label),
    legend: { position: "top" },
    dataLabels: {
        enabled: true,
        // v pie/donut jsou dataLabels procenta výseče (ne count) citeturn0search0
        formatter: (val: number) => `${Math.round(val)}%`,
    },
}));

// 100% stacked bar (skóre) – chart.stacked + stackType 100% citeturn0search1
const stackedSeries = computed(() =>
    scoreDist.value.map(d => ({ name: d.label, data: [Number(d.count) || 0] }))
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
    xaxis: { categories: ["Rozložení skóre"], labels: { show: false } },
    yaxis: { labels: { show: false } },
    legend: { position: "top" },
    dataLabels: { enabled: true, formatter: (val: number) => `${Math.round(val)}%` },
}));

// čas – tvoje “timeDistribution” je bucketované, bar by byl často čitelnější,
// ale line nechám (jen český popisek)
const lineSeries = computed(() => [
    { name: "Počet pokusů", data: timeDist.value.map(x => Number(x.count) || 0) },
]);

const lineOptions = computed<ApexOptions>(() => ({
    chart: { type: "line" },
    stroke: { curve: "smooth", width: 4 },
    xaxis: { categories: timeDist.value.map(x => x.label) },
    legend: { position: "top" },
}));

// vysvětlivky – čistě text, žádná vymyšlená data
const scoreBucketsHelp = computed(() => {
    return "Intervaly skóre udávají, kolik % otázek bylo v jednom pokusu zodpovězeno správně. " +
        "Např. „20–40 %“ znamená pokusy s úspěšností alespoň 20 % a menší než 40 % (dle logiky na backendu).";
});

const timeBucketsHelp = computed(() => {
    return "Intervaly času udávají, do jakého časového rozmezí spadla doba vyplňování kvízu (v sekundách převedená na minuty).";
});
</script>

<template>
    <Modal
        v-if="enabled"
        :enabled="enabled"
        can-be-closed-by-clicking-outside
        :modal-style="{ maxWidth: '1080px' }"
        @close="emit('close')"
        
    >
        <div v-if="!hasQuizInfo" style="padding: 16px; font-family: 'Dosis', sans-serif; font-size: 18px; font-weight: 700">
            K tomuto kvízu zatím nejsou žádné informace (žádné odevzdané pokusy).
        </div>

        <div v-else :class="$style.quizResultsModal">
            <h3>Výsledky kvízu</h3>

            <!-- INFO -->
            <div :class="$style.infoGrid">
                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Počet pokusů</div>
                    <div :class="$style.infoValue">{{ totalAttempts }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Průměrné skóre</div>
                    <div :class="$style.infoValue">{{ round2(averageScore) }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Průměrná úspěšnost</div>
                    <div :class="$style.infoValue">{{ Math.round(averageScorePercentage) }} %</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Průměrný čas vyplňování</div>
                    <div :class="$style.infoValue">{{ formatSeconds(averageTimeSpent) }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Nejčastější interval skóre</div>
                    <div :class="$style.infoValue">{{ mostCommonScoreBucket }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">Nejčastější interval času</div>
                    <div :class="$style.infoValue">{{ mostCommonTimeBucket }}</div>
                </div>
            </div>

            <!-- Diagnostika dat z API (užitečné hlavně kvůli bucket bugům) -->
            <p v-if="scoreBucketsTotal !== totalAttempts" style="opacity:.75; margin: 0;">
                Pozn.: Součet rozložení skóre ({{ scoreBucketsTotal }}) neodpovídá počtu pokusů ({{ totalAttempts }}).
                To typicky znamená, že některé výsledky nespadly do žádného intervalu (např. 100% úspěšnost).
            </p>

            <!-- Vysvětlivky -->
            <div style="opacity: .85;">
                <p style="margin: 8px 0 0;"><strong>Co znamenají intervaly?</strong></p>
                <p style="margin: 4px 0 0;">{{ scoreBucketsHelp }}</p>
                <p style="margin: 4px 0 0;">{{ timeBucketsHelp }}</p>
            </div>

            <!-- CHARTS -->
            <div :class="$style.chartsGrid" style="margin-top: 12px;">
                <div :class="$style.chartCard">
                    <h4>Rozložení času</h4>

                    <div v-if="!canRenderTimeChart" style="opacity:.8;">
                        Rozložení času není k dispozici.
                    </div>

                    <VueApexCharts
                        v-else
                        type="line"
                        :series="lineSeries"
                        :options="lineOptions"
                        height="320"
                    />
                </div>

                <div :class="$style.chartCard">
                    <h4>Rozložení skóre</h4>

                    <div v-if="!canRenderScoreCharts" style="opacity:.8; font-family: Dosis, sans-serif;">
                        Rozložení skóre není k dispozici (součet bucketů je 0).
                    </div>

                    <VueApexCharts
                        v-else
                        type="pie"
                        :series="pieSeries"
                        :options="pieOptions"
                        height="320"
                    />
                </div>

<!--                <div :class="$style.chartCard">-->
<!--                    <h4>Rozložení skóre (100% skládaný graf)</h4>-->

<!--                    <div v-if="!canRenderScoreCharts" style="opacity:.8; font-family: Dosis, sans-serif;">-->
<!--                        100% skládaný graf nelze vykreslit, když je součet 0.-->
<!--                    </div>-->

<!--                    <VueApexCharts-->
<!--                        v-else-->
<!--                        type="bar"-->
<!--                        :series="stackedSeries"-->
<!--                        :options="stackedOptions"-->
<!--                        height="220"-->
<!--                    />-->
<!--                </div>-->
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
    font-size: 18px;
    color: var(--text-color-secondary);
    font-family: "Dosis" , sans-serif;
    margin-bottom: 6px;
}

.infoValue {
    font-size: 24px;
    font-weight: 700;
    font-family: "Dosis" , sans-serif;
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
