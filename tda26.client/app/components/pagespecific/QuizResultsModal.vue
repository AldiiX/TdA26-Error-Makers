<script setup lang="ts">
import type {QuizResultsSummary} from "#shared/types";
import Modal from "~/components/Modal.vue";
import VueApexCharts from "vue3-apexcharts";


const props = defineProps<{
    quizResultsSummary: QuizResultsSummary | null,
    enabled: boolean,
}>();

const emit = defineEmits<{
    (e: "close"): void;
}>();

watch(() => props.quizResultsSummary, (results) => {
    console.log("Received quiz results summary:", results);
});



const series = ref([{ name: "Score", data: [10, 20, 15, 30, 25] }]);
const options = ref({
    chart: { type: "line" as const },
    xaxis: { categories: [5,6,7,8,9] }
});

const scoreDistributionChartElement = ref<HTMLCanvasElement | null>(null);
const timeDistributionChartElement = ref<HTMLCanvasElement | null>(null);

watch(scoreDistributionChartElement, (element) => {
    if (!element || !(element instanceof HTMLCanvasElement)) return;

    const data = (props.quizResultsSummary?.scoreDistribution
        .filter(item => item.count > 0)
        .map(item => ({
            label: item.label,
            count: item.count
        })) || []);

    console.log("Score distribution data for chart:", data);
    
});

watch(timeDistributionChartElement, (element) => {
    if (!element || !(element instanceof HTMLCanvasElement) || props.quizResultsSummary?.timeDistribution === undefined) return;

    const data = (props.quizResultsSummary.timeDistribution
        .filter(item => item.count > 0)
        .map(item => ({
            label: item.label,
            count: item.count
        })) || []);

    console.log("Score distribution data for chart:", data);

    
});
</script>

<template>
    <Modal
        v-if="quizResultsSummary"
        :enabled="enabled"
        can-be-closed-by-clicking-outside
        :modal-style="{ maxWidth: '1080px' }"
        @close="emit('close')"
    >
        <VueApexCharts type="line" :series="series" :options="options" />
    </Modal>
</template>

<style module lang="scss">
.quizResultsModal {
    display: flex;
    flex-direction: column;
    gap: 16px;

    >h3 {
        margin-bottom: 12px !important;
    }

    >.info {
        display: flex;
        gap: 12px;
        flex-direction: column;

        p {
            margin: 0;
        }
    }

    .charts {
        display: flex;
        gap: 24px;
        flex-wrap: wrap;

        >div {
            flex: 1;
            display: flex;
            flex-direction: column;
            gap: 8px;
            max-width: 300px;
            max-height: 300px;

            h4 {
                margin: 0;
                font-size: 24px;
                text-align: center;
            }
        }
    }
}
</style>
