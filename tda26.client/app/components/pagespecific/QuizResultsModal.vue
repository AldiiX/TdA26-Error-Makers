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

const isHelpOpen = ref(false);

function openHelp() {
    isHelpOpen.value = true;
}

function closeHelp() {
    isHelpOpen.value = false;
}

const CHART_COLORS = [
    "var(--accent-color-secondary)",
    "var(--accent-color-additional-1)",
    "var(--accent-color-additional-2)",
    "var(--accent-color-additional-3)",
    "var(--accent-color-additional-4)",
    "var(--accent-color-primary)",
];

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
    colors: CHART_COLORS,
    dataLabels: {
        enabled: true,
        formatter: (val: number) => `${Math.round(val)}%`,
    },
    theme: { mode: "light" }, // pokud máš dark mode, můžeš dát computed podle theme
}));

const stackedOptions = computed<ApexOptions>(() => ({
    chart: {
        type: "bar",
        stacked: true,
        stackType: "100%",
        toolbar: { show: false },
    },
    colors: CHART_COLORS,
    plotOptions: { bar: { horizontal: true, barHeight: "40%" } },
    xaxis: { categories: ["Rozložení skóre"], labels: { show: false } },
    yaxis: { labels: { show: false } },
    legend: { position: "top" },
    dataLabels: { enabled: true, formatter: (val: number) => `${Math.round(val)}%` },
}));

const lineOptions = computed<ApexOptions>(() => ({
    chart: { type: "line", toolbar: { show: false } },
    colors: ["var(--accent-color-primary)"],
    stroke: { curve: "smooth", width: 4 },
    xaxis: { categories: timeDist.value.map(x => x.label) },
    legend: { position: "top" },
}));
const lineSeries = computed(() => [
    { name: "Počet pokusů", data: timeDist.value.map(x => Number(x.count) || 0) },
]);


// vysvětlivky – čistě text, žádná vymyšlená data
const scoreBucketsHelp = computed(() => {
    return "Intervaly skóre udávají, kolik % otázek bylo v jednom pokusu zodpovězeno správně. " +
        "Např. „20–40%“ znamená pokusy s úspěšností alespoň 20% a menší než 40% (dle logiky na backendu).";
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
            <div :class="$style.helpRow">
                <button type="button" :class="$style.helpButton" @click="openHelp">
                    <div :class="$style.helpIcon"></div>
                </button>
            </div>
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
            </div>
        </div>
    </Modal>

    <Modal
        v-if="enabled && isHelpOpen"
        :enabled="enabled && isHelpOpen"
        can-be-closed-by-clicking-outside
        :modal-style="{ maxWidth: '720px' }"
        @close="closeHelp"
    >
        <div :class="$style.helpModal">
            <div :class="$style.helpModalBody">
                <div :class="$style.helpSection">
                    <div :class="$style.helpSectionTitle">Intervaly skóre</div>
                    <p :class="$style.helpText">{{ scoreBucketsHelp }}</p>
                </div>

                <div :class="$style.helpSection">
                    <div :class="$style.helpSectionTitle">Intervaly času</div>
                    <p :class="$style.helpText">{{ timeBucketsHelp }}</p>
                </div>

                <div :class="$style.helpFooter">
                    <button type="button" :class="$style.helpButtonSecondary" @click="closeHelp">
                        Zavřít
                    </button>
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
    font-size: 16px;
    color: var(--text-color-secondary);
    font-family: "Dosis" , sans-serif;
    margin-bottom: 6px;
    font-weight: 500;
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

    :global(.apexcharts-legend-text) {
        font-family: "Dosis", sans-serif !important;
        font-weight: 500 !important;
    }

    h4 {
        margin: 0 0 10px;
        font-size: 16px;
    }
}

.helpRow {
    display: flex;
    justify-content: flex-start;
    margin-top: -6px;
}

.helpButton {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 14%, transparent 40%);
    background: color-mix(in srgb, var(--background-color-secondary) 80%, transparent);
    border-radius: 999px;
    padding: 6px;
    font-family: "Dosis", sans-serif;
    font-weight: 700;
    font-size: 14px;
    cursor: pointer;
    transition: background-color .2s, border-color .2s;
    color: var(--text-color);
    justify-self: right;

    &:hover {
        background: color-mix(in srgb, var(--accent-color-primary) 8%, var(--background-color-secondary) 92%);
        border-color: color-mix(in srgb, var(--accent-color-primary) 25%, transparent);
    }
}

/* --- Help modal --- */
.helpModal {
    padding: 14px 14px 12px;
}

.helpIcon {
    width: 28px;
    height: 28px;
    background-color: var(--color-gray);
    mask-image: url('/icons/info.svg');
    mask-size: cover;
    mask-position: center;
    mask-repeat: no-repeat;
}

.helpModalTitle {
    margin: 0;
    font-size: 20px;
    font-family: "Dosis", sans-serif;
}

.helpClose {
    background: none;
    border: 1px solid transparent;
    cursor: pointer;
    border-radius: 10px;
    padding: 6px 10px;
    font-size: 16px;
    line-height: 1;
    opacity: .8;

    &:hover {
        opacity: 1;
        background: color-mix(in srgb, var(--text-color-secondary) 10%, transparent);
        border-color: color-mix(in srgb, var(--text-color-secondary) 20%, transparent);
    }
}

.helpModalBody {
    padding-top: 12px;
    display: flex;
    flex-direction: column;
    gap: 14px;
}

.helpSection {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 10%, transparent 40%);
    border-radius: 12px;
    padding: 12px;
    background: var(--background-color-secondary, #fff);
}

.helpSectionTitle {
    font-family: "Dosis", sans-serif;
    font-weight: 800;
    font-size: 24px;
    margin-bottom: 6px;
}

.helpText {
    margin: 0;
    font-size: 16px;
    line-height: 1.45;
    color: var(--text-color);
    white-space: pre-wrap;
}

.helpFooter {
    display: flex;
    justify-content: flex-end;
    padding-top: 2px;
}

.helpButtonSecondary {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 14%, transparent 40%);
    background: transparent;
    border-radius: 10px;
    padding: 8px 12px;
    cursor: pointer;
    font-family: "Dosis", sans-serif;
    font-weight: 800;
}

/* --- Mobile design --- */
@media (max-width: 900px) {
    .helpRow {
        justify-content: stretch;
    }

    .helpButton {
        text-align: center;
        font-size: 15px;
    }

    .helpModal {
        padding: 10px;
    }

    .helpModalTitle {
        font-size: 18px;
    }

    .helpSection {
        padding: 10px;
    }
}
</style>
