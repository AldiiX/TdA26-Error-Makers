<script setup lang="ts">
import Modal from "~/components/Modal.vue";
import type {MaterialResultsSummary} from "#shared/types";
import VueApexCharts from "vue3-apexcharts";
import type { ApexOptions } from "apexcharts";
import { computed } from "vue";



const props = defineProps<{
    enabled: boolean;
    materialResultsSummary: MaterialResultsSummary | null;
}>();


const CHART_COLORS = [
    "var(--accent-color-additional-1)",
    "var(--accent-color-additional-2)",
    "var(--accent-color-additional-3)",
    "var(--accent-color-additional-4)",
    "var(--accent-color-secondary)",
    "var(--accent-color-primary)",
];



const emit = defineEmits<{ (e: "close"): void }>();

const summary = computed(() => props.materialResultsSummary);
const hasInfo = computed(() => {
    if (!summary.value) return false;
    if (summary.value.type === "url") return (summary.value.clickCount ?? 0) > 0;
    return (summary.value.downloadCount ?? 0) > 0;
});

function formatDateTime(dt?: string | null) {
    if (!dt) return "—";
    const d = new Date(dt);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleString();
}

function formatBytes(bytes?: number | null) {
    const b = Number(bytes ?? 0);
    if (!Number.isFinite(b) || b <= 0) return "0 B";
    const units = ["B", "KB", "MB", "GB", "TB"];
    let i = 0;
    let v = b;
    while (v >= 1024 && i < units.length - 1) {
        v /= 1024;
        i++;
    }
    const rounded = v >= 100 ? Math.round(v) : Math.round(v * 10) / 10;
    return `${rounded} ${units[i]}`;
}

function round2(n: number) {
    return Math.round(n * 100) / 100;
}

const title = computed(() => {
    if (!summary.value) return "Statistiky materiálu";
    return summary.value.type === "url" ? "Statistiky odkazového materiálu" : "Statistiky souborového materiálu";
});

const primaryCountLabel = computed(() => {
    if (!summary.value) return "Interakce";
    return summary.value.type === "url" ? "Počet kliknutí" : "Počet stažení";
});

const primaryCountValue = computed(() => {
    if (!summary.value) return 0;
    return summary.value.type === "url" ? summary.value.clickCount : summary.value.downloadCount;
});

const lastInteractionLabel = computed(() => {
    if (!summary.value) return "Poslední interakce";
    return summary.value.type === "url" ? "Poslední kliknutí" : "Poslední stažení";
});

const lastInteractionValue = computed(() => {
    if (!summary.value) return "—";
    return summary.value.type === "url"
        ? formatDateTime(summary.value.lastClickedAt)
        : formatDateTime(summary.value.lastDownloadedAt);
});


const canRenderCharts = computed(() => hasInfo.value);
const donutSeries = computed<number[]>(() => {
    const v = Number(primaryCountValue.value ?? 0) || 0;
    return v > 0 ? [v, 0] : [0, 0];
});

const donutOptions = computed<ApexOptions>(() => ({
    labels: [primaryCountLabel.value, "—"],
    legend: { position: "top" },
    colors: [
        "var(--accent-color-primary)",
        "var(--accent-color-additional-1)"
    ],
    stroke: { width: 0 },
    dataLabels: {
        enabled: true,
        formatter: (_val: number, opts: any) => {
            const idx = opts?.seriesIndex ?? 0;
            if (idx === 0) return `${primaryCountValue.value}`;
            return "";
        },
    },
}));
const barSeries = computed(() => {
    if (!summary.value) return [{ name: "Hodnota", data: [0, 0] }];

    if (summary.value.type === "file") {
        const totalMB = Number(summary.value.totalMegabytesDownloaded ?? 0) || 0;
        const avgMB = Number(summary.value.averageMegabytesPerDownload ?? 0) || 0;
        return [{ name: "MB", data: [Math.round(totalMB * 100) / 100, Math.round(avgMB * 100) / 100] }];
    }

    // url
    const clicks = Number(summary.value.clickCount ?? 0) || 0;
    return [{ name: "Kliknutí", data: [clicks, 1] }];
});

const barOptions = computed<ApexOptions>(() => ({
    chart: { type: "bar", toolbar: { show: false } },
    colors: [
        "var(--accent-color-additional-3)",
    ],
    plotOptions: {
        bar: {
            borderRadius: 8,
            columnWidth: "50%",
        },
    },
    dataLabels: { enabled: true },
    xaxis: {
        categories:
            summary.value?.type === "file"
                ? ["Celkem MB", "Průměr / stažení (MB)"]
                : ["Kliknutí", "Interakce"],
    },
    legend: { show: false },
}));

const chartColors = [
    "var(--accent-color-additional-1)",
    "var(--accent-color-additional-2)",
    "var(--accent-color-additional-3)",
    "var(--accent-color-additional-4)",
    "var(--accent-color-primary)",
    "var(--accent-color-secondary)",
];

const clickCount = computed(() => summary.value?.type === "url" ? (summary.value.clickCount ?? 0) : 0);

const daysSinceLastClick = computed(() => {
    if (!summary.value || summary.value.type !== "url") return 0;
    const dt = summary.value.lastClickedAt ? new Date(summary.value.lastClickedAt).getTime() : NaN;
    if (!Number.isFinite(dt)) return 0;
    const diffMs = Date.now() - dt;
    return Math.max(0, Math.round(diffMs / (1000 * 60 * 60 * 24)));
});

// Graf 1: radial bar = clickCount
const urlRadialSeries = computed<number[]>(() => [clickCount.value]);

const urlRadialOptions = computed<ApexOptions>(() => ({
    chart: { type: "radialBar", toolbar: { show: false } },
    colors: ["var(--accent-color-additional-2)"],
    labels: ["Kliknutí"],
    plotOptions: {
        radialBar: {
            dataLabels: {
                name: { fontSize: "14px" },
                value: { fontSize: "24px", fontWeight: 700 },
            },
        },
    },
}));

// Graf 2: bar = daysSinceLastClick
const urlDaysSeries = computed(() => [
    { name: "Dní", data: [daysSinceLastClick.value] },
]);

const urlDaysOptions = computed<ApexOptions>(() => ({
    chart: { type: "bar", toolbar: { show: false } },
    colors: ["var(--accent-color-additional-4)"],
    plotOptions: { bar: { borderRadius: 8, columnWidth: "45%" } },
    xaxis: { categories: ["Od posledního kliknutí"] },
    yaxis: { min: 0, forceNiceScale: true },
    dataLabels: { enabled: true },
}));
</script>

<template>
    <Modal
        v-if="enabled"
        :enabled="enabled"
        can-be-closed-by-clicking-outside
        :modal-style="{ maxWidth: '900px' }"
        @close="emit('close')"
    >
        <div v-if="!summary" style="padding: 16px; font-family: 'Dosis', sans-serif; font-size: 18px; font-weight: 700">
            Statistiky materiálu nejsou k dispozici.
        </div>

        <div v-else :class="$style.materialResultsModal">
            <h3>{{ title }}</h3>

            <div v-if="!hasInfo" style="padding: 8px 0; opacity: .9; font-family: 'Dosis', sans-serif; font-size: 18px; font-weight: 700">
                K tomuto materiálu zatím nejsou žádné informace (žádné kliknutí / stažení).
            </div>

            <div v-else :class="$style.infoGrid">
                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">{{ primaryCountLabel }}</div>
                    <div :class="$style.infoValue">{{ primaryCountValue }}</div>
                </div>

                <div :class="$style.infoCard">
                    <div :class="$style.infoLabel">{{ lastInteractionLabel }}</div>
                    <div :class="$style.infoValue">{{ lastInteractionValue }}</div>
                </div>

                <!-- URL specific -->
                <template v-if="summary.type === 'url'">
                    <div :class="$style.infoCard">
                        <div :class="$style.infoLabel">Typ</div>
                        <div :class="$style.infoValue">Odkaz</div>
                    </div>

                    <div v-if="hasInfo && summary.type === 'url'" :class="$style.chartsGrid" style="margin-top: 12px;">
                        <div :class="$style.chartCard">
                            <h4>Počet kliknutí</h4>
                            <VueApexCharts
                                type="radialBar"
                                :series="urlRadialSeries"
                                :options="urlRadialOptions"
                                height="300"
                            />
                        </div>

                        <div :class="$style.chartCard">
                            <h4>Aktualita</h4>
                            <VueApexCharts
                                type="bar"
                                :series="urlDaysSeries"
                                :options="urlDaysOptions"
                                height="300"
                            />
                        </div>
                    </div>
                </template>

                <!-- FILE specific -->
                <template v-else>
                    <div :class="$style.infoCard">
                        <div :class="$style.infoLabel">Velikost souboru</div>
                        <div :class="$style.infoValue">{{ formatBytes(summary.sizeBytes) }}</div>
                    </div>

                    <div :class="$style.infoCard">
                        <div :class="$style.infoLabel">Celkem staženo</div>
                        <div :class="$style.infoValue">{{ formatBytes(summary.totalBytesDownloaded) }}</div>
                    </div>

                    <div :class="$style.infoCard">
                        <div :class="$style.infoLabel">Celkem přeneseno</div>
                        <div :class="$style.infoValue">{{ round2(summary.totalMegabytesDownloaded) }} MB</div>
                    </div>

                    <div :class="$style.infoCard">
                        <div :class="$style.infoLabel">Průměr na stažení</div>
                        <div :class="$style.infoValue">{{ round2(summary.averageMegabytesPerDownload) }} MB</div>
                    </div>

                    <!-- CHARTS -->
                    <div v-if="hasInfo" :class="$style.chartsGrid">
                        <div :class="$style.chartCard">
                            <h4>{{ primaryCountLabel }}</h4>
                            <VueApexCharts
                                type="donut"
                                :series="donutSeries"
                                :options="donutOptions"
                                height="260"
                            />
                        </div>

                        <div :class="$style.chartCard">
                            <h4>{{ summary.type === "file" ? "Přenosy" : "Aktivita" }}</h4>
                            <VueApexCharts
                                type="bar"
                                :series="barSeries"
                                :options="barOptions"
                                height="260"
                            />
                        </div>
                    </div>
                </template>
            </div>
        </div>
    </Modal>
</template>

<style module lang="scss">
.materialResultsModal {
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

.infoCard {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 10%, transparent 40%);
    border-radius: 12px;
    padding: 12px;
    background: var(--background-color-secondary, #fff);
}

.infoLabel {
    font-size: 18px;
    color: var(--text-color-secondary);
    font-family: "Dosis", sans-serif;
    margin-bottom: 6px;
}

.infoValue {
    font-size: 24px;
    font-weight: 700;
    font-family: "Dosis", sans-serif;
}
</style>