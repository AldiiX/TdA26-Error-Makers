<script setup lang="ts">
import type {QuizResultsSummary} from "#shared/types";
import Modal from "~/components/Modal.vue";
import { Chart } from 'chart.js/auto';

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

    new Chart(
        element,
        {
            type: 'pie',
            data: {
                labels: data.map(row => row.label),
                datasets: [
                    {
                        label: 'Počet pokusů',
                        data: data.map(row => row.count)
                    }
                ]
            }
        }
    );
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

    new Chart(
        element,
        {
            type: 'pie',
            data: {
                labels: data.map(row => row.label),
                datasets: [
                    {
                        label: 'Počet pokusů',
                        data: data.map(row => row.count)
                    }
                ]
            }
        }
    );
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
        <div :class="$style.quizResultsModal">
            <h3>Výsledky kvízu</h3>
            <div :class="$style.info">
                <p>Průměrný strávený čas: {{ toClockTime(quizResultsSummary.averageTimeSpent) }}</p>
                <p>Průměrné skóre: {{ Math.round(quizResultsSummary.averageScore * 100) / 100 }} ({{ Math.round(quizResultsSummary.averageScorePercentage) }}%)</p>
            </div>

            <div :class="$style.charts">
                <div :class="$style.scoreDistribution">
                    <h4>Distribuce skóre</h4>
                    <canvas ref="scoreDistributionChartElement"></canvas>
                </div>
                <div :class="$style.timeDistribution">
                    <h4>Distribuce času</h4>
                    <canvas ref="timeDistributionChartElement"></canvas>
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
