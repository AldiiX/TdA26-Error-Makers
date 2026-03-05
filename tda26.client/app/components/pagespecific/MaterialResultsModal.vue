<script setup lang="ts">
import Modal from "~/components/Modal.vue";
import type { MaterialResultsSummary } from "#shared/types";
import VueApexCharts from "vue3-apexcharts";
import type { ApexOptions } from "apexcharts";
import { computed } from "vue";
import { push } from "notivue";


const APEX_FONT = "Dosis, sans-serif";

const urlCurveOptions = computed<ApexOptions>(() => ({
    chart: {
        id: "material-url-clicks-curve",
        type: "area",
        width: "100%",
        fontFamily: APEX_FONT,          // ✅ Apex celkově
        toolbar: exportToolbar("material-url-clicks"),
        events: {
            dataPointSelection: (_e, _ctx, cfg) => {
                const idx = cfg?.dataPointIndex ?? -1;
                if (idx < 0) return;
                const label = urlCurveCategories[idx] ?? "—";
                const value = Number(urlCurveSeries.value?.[0]?.data?.[idx] ?? 0);
                onPointClick(label, value);
            },
        },
    },

    colors: [CHART_COLORS[2]],
    stroke: { curve: "smooth", width: 4 },

    xaxis: {
        categories: ["Start", "Teď"],
        labels: {
            style: {
                fontFamily: APEX_FONT,       // ✅ Start/Teď v Dose
                fontSize: "14px",
                colors: "var(--text-color-secondary)",
            } as any,
        },
    },

    yaxis: {
        min: 0,
        forceNiceScale: true,
        labels: {
            style: {
                fontFamily: APEX_FONT,       // ✅ osa Y v Dose
                fontSize: "14px",
                colors: "var(--text-color-secondary)",
            } as any,
        },
    },

    legend: {
        position: "top",
        fontFamily: APEX_FONT,          
    },

    tooltip: {
        style: { fontFamily: APEX_FONT }, 
        y: { formatter: (v: number) => `${Math.round(v)}` },
    },

    markers: { size: 5 },
    dataLabels: { enabled: false },

    fill: {
        type: "gradient",
        gradient: {
            shadeIntensity: 0.6,
            opacityFrom: 0.35,
            opacityTo: 0.02,
            stops: [0, 90, 100],
        },
    },

    grid: {
        borderColor: "color-mix(in srgb, var(--text-color-secondary) 12%, transparent)",
    },
}));

const props = defineProps<{
    enabled: boolean;
    materialResultsSummary: MaterialResultsSummary | null;
}>();

const emit = defineEmits<{ (e: "close"): void }>();

const CHART_COLORS = [
    "var(--accent-color-additional-1)",
    "var(--accent-color-additional-2)",
    "var(--accent-color-additional-3)",
    "var(--accent-color-additional-4)",
    "var(--accent-color-secondary)",
    "var(--accent-color-primary)",
];

const summary = computed(() => props.materialResultsSummary);

const hasInfo = computed(() => {
    if (!summary.value) return false;
    if (summary.value.type === "url") return (summary.value.clickCount ?? 0) > 0;
    return (summary.value.downloadCount ?? 0) > 0;
});

// +1h timezone fix
function formatDateTime(dt?: string | null) {
    if (!dt) return "—";
    const d = new Date(dt);
    if (Number.isNaN(d.getTime())) return "—";
    d.setHours(d.getHours() + 1);
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
    return summary.value.type === "url"
        ? "Statistiky odkazového materiálu"
        : "Statistiky souborového materiálu";
});

const primaryCountLabel = computed(() => {
    if (!summary.value) return "Interakce";
    return summary.value.type === "url" ? "Počet kliknutí" : "Počet otevření";
});

const primaryCountValue = computed(() => {
    if (!summary.value) return 0;
    return summary.value.type === "url" ? summary.value.clickCount : summary.value.downloadCount;
});

const lastInteractionLabel = computed(() => {
    if (!summary.value) return "Poslední interakce";
    return summary.value.type === "url" ? "Poslední kliknutí" : "Poslední otevření";
});

const lastInteractionValue = computed(() => {
    if (!summary.value) return "—";
    return summary.value.type === "url"
        ? formatDateTime(summary.value.lastClickedAt)
        : formatDateTime(summary.value.lastDownloadedAt);
});

// ---- common helpers for chart UX ----
function onPointClick(label: string, value: number) {
    push.info({
        title: "Detail",
        message: `${label}: ${value}`,
        duration: 2500,
    });
}

function exportToolbar(filenameBase: string) {
    return {
        show: true,
        tools: {
            download: true,
            selection: false,
            zoom: false,
            zoomin: false,
            zoomout: false,
            pan: false,
            reset: false,
        },
        export: {
            png: { filename: filenameBase },
            svg: { filename: filenameBase },
            csv: { filename: `${filenameBase}-data` },
        },
    } as const;
}

// ---- FILE charts ----
// Donut: "počet stažení" – jen jako vizuální badge (bez % iluze: label ukáže count)
const donutSeries = computed<number[]>(() => {
    const v = Number(primaryCountValue.value ?? 0) || 0;
    return v > 0 ? [v, 0] : [0, 0];
});

const donutOptions = computed<ApexOptions>(() => ({
    chart: {
        type: "donut",
        toolbar: exportToolbar("material-file-downloads"),
        events: {
            dataPointSelection: (_e, _ctx, cfg) => {
                const idx = cfg?.seriesIndex ?? -1;
                if (idx !== 0) return;
                onPointClick(primaryCountLabel.value, Number(primaryCountValue.value ?? 0));
            },
        },
    },
    labels: [primaryCountLabel.value, "—"],
    legend: { position: "top" },
    colors: [CHART_COLORS[5], CHART_COLORS[0]],
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
    if (!summary.value) return [{ name: "MB", data: [0, 0] }];

    if (summary.value.type === "file") {
        const totalMB = Number(summary.value.totalMegabytesDownloaded ?? 0) || 0;
        const avgMB = Number(summary.value.averageMegabytesPerDownload ?? 0) || 0;
        return [
            {
                name: "Objem dat (MB)",
                data: [Math.round(totalMB * 100) / 100, Math.round(avgMB * 100) / 100],
            },
        ];
    }

    // url se sem nedostane (bar používáme jen ve file části)
    return [{ name: "MB", data: [0, 0] }];
});

const barCategories = computed(() =>
    summary.value?.type === "file"
        ? ["Celkem otevřených dat (MB)", "Průměr na otevření (MB)"]
        : ["—", "—"]
);

const barOptions = computed<ApexOptions>(() => ({
    chart: {
        type: "bar",
        toolbar: exportToolbar("material-file-transfers"),
        events: {
            dataPointSelection: (_e, _ctx, cfg) => {
                const idx = cfg?.dataPointIndex ?? -1;
                if (idx < 0) return;
                const label = barCategories.value[idx] ?? "—";
                const value = Number(barSeries.value?.[0]?.data?.[idx] ?? 0);
                onPointClick(label, value);
            },
        },
    },
    colors: [CHART_COLORS[2]],
    plotOptions: {
        bar: { borderRadius: 8, columnWidth: "50%" },
    },
    dataLabels: { enabled: true },
    xaxis: { categories: barCategories.value },
    legend: { show: false },
}));

// ---- URL chart (100% width, clickable + downloadable) ----
const urlClickCount = computed(() => {
    if (summary.value?.type !== "url") return 0;
    return Number(summary.value.clickCount ?? 0) || 0;
});

const urlCurveSeries = computed(() => [
    { name: "Kliknutí", data: [0, urlClickCount.value] },
]);

const urlCurveCategories = ["Start", "Teď"];

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

                    <!-- URL CHART: přes celou šířku -->
                    <div v-if="hasInfo" :class="[$style.urlChartsGrid, $style.fullRow]" style="margin-top: 12px;">
                        <div :class="$style.chartCard" style="width: 100%;">
                            <h4>Křivka kliknutí</h4>
                            <VueApexCharts
                                type="area"
                                :series="urlCurveSeries"
                                :options="urlCurveOptions"
                                height="300"
                                style="width: 100%;"
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
                        <div :class="$style.infoLabel">Celkem otevřených dat</div>
                        <div :class="$style.infoValue">{{ formatBytes(summary.totalBytesDownloaded) }}</div>
                    </div>

                    <!-- CHARTS -->
                    <div v-if="hasInfo" :class="[$style.chartsGrid, $style.fullRow]">
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
                            <h4>{{ summary.type === "file" ? "Objem otevřených dat" : "Aktivita" }}</h4>
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

.fullRow {
    grid-column: 1 / -1; 
}

.infoGrid {
    display: grid;
    gap: 12px;
    grid-template-columns: repeat(2, minmax(0, 1fr));

    @media (max-width: 900px) {
        grid-template-columns: 1fr;
    }
}

.urlChartsGrid{
    display: grid;
    gap: 16px;
    grid-template-columns: 1fr;

    @media (max-width: 900px) {
        grid-template-columns: 1fr;
    }
}

.chartsGrid {
    display: flex;
    gap: 16px;
    width: 100%;
    align-items: stretch;

    @media (max-width: 600px) {
        flex-direction: column;
    }
}

.chartCard {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 10%, transparent 40%);
    border-radius: 12px;
    padding: 12px;
    background: var(--background-color-secondary, #fff);

    flex: 1 1 0;
    min-width: 0;

    h4 {
        margin: 0 0 10px;
        font-size: 16px;
    }
}

.chartCard :global(.apexcharts-canvas),
.chartCard :global(.apexcharts-svg) {
    width: 100% !important;
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