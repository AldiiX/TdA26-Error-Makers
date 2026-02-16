<script setup lang="ts">
import type {Quiz, QuizResultsSummary} from "#shared/types";
import { NuxtLink } from '#components';
import Button from "~/components/Button.vue";
import Modal from "~/components/Modal.vue";
import { useCourseDialogs } from "~/composables/courses/[uuid]/useCourseDialogs";
import toClockTime from "~/utils/toClockTime";
import { Chart } from 'chart.js/auto';

const props = defineProps<{
    quiz: Quiz,
    course: { uuid: string },
    editMode?: boolean
}>();

const { enabledModal } = useCourseDialogs();

function openResults() {
    enabledModal.value = "quizResults";
}

const emit = defineEmits<{
    (e: "edit", quiz: Quiz): void;
    (e: "delete", quiz: Quiz): void;
}>();

const quizResultsSummary = ref<QuizResultsSummary | null>(null);
const scoreDistributionChartElement = ref<HTMLCanvasElement | null>(null);
const timeDistributionChartElement = ref<HTMLCanvasElement | null>(null);

onMounted(() => {
    fetch(`/api/v2/courses/${props.course.uuid}/quizzes/${props.quiz.uuid}/results-summary`)
        .then(res => {
            if (!res.ok) throw new Error("Failed to fetch quiz results summary");
            return res.json();
        })
        .then(data => {
            quizResultsSummary.value = data;
        })
        .catch(err => {
            console.error("Error fetching quiz results summary:", err);
        });
});

watch(scoreDistributionChartElement, (element) => {
    if (!element || !(element instanceof HTMLCanvasElement)) return;
    
    const data = (quizResultsSummary.value?.scoreDistribution
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
    if (!element || !(element instanceof HTMLCanvasElement) || quizResultsSummary.value?.timeDistribution === undefined) return;
    
    const data = (quizResultsSummary.value.timeDistribution
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
    <div :class="$style.element">
        <NuxtLink :href="`/courses/${course.uuid}/quiz/${quiz.uuid}`" :class="$style.info">
<!--                <div :class="$style.favicon">-->
<!--                    <img v-if="quiz.faviconUrl" :src="quiz.faviconUrl" alt="Favicon" />-->
<!--                </div>-->

            <div :class="$style.specificInfo">
                <p :title="quiz.title">{{ quiz.title }}</p>
                <div :class="$style.fileDetails">
                <p>{{ new Date(quiz.createdAt).toLocaleDateString() }}</p>
                </div>
            </div>
            <span :class="$style.divider"/>
            <div :class="$style.description">
                <p>{{ quiz.attemptsCount }} dokončení</p>
            </div>
<!--                <p :class="$style.description">{{ quiz.description }}</p>-->
        </NuxtLink>
        
        <div v-if="editMode" :class="$style.editButtons">
            <Button @click="openResults">Výsledky</Button>
            <NuxtLink :href="`/courses/${course.uuid}/quiz/${quiz.uuid}/edit`">
                <Button button-style="primary" accent-color="secondary" style="width: 100%">Upravit</Button>
            </NuxtLink>
            <Button button-style="secondary" accent-color="secondary" style="width: 100%" @click="emit('delete', quiz)">Smazat</Button>
        </div>

    </div>
    
    <Teleport to="#teleports">
        <Modal
            v-if="quizResultsSummary"
            :enabled="enabledModal === 'quizResults'"
            can-be-closed-by-clicking-outside
            :modal-style="{ maxWidth: '1080px' }"
            @close="enabledModal = null"
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

    </Teleport>
    
</template>

<style module lang="scss">
.element {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 12px;
    min-height: 72px;
    
    //box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 10%, transparent 40%);
    border-radius: 12px;
    transition: background-color 0.3s;

    &:hover {
        background-color: color-mix(in srgb, var(--accent-color-primary) 5%, var(--background-color-secondary) 95%);
    }
    
    .divider {
        width: 1px;
        height: 36px;
        border-right: 1px solid color-mix(in srgb, var(--text-color-secondary) 20%, transparent 40%);
        display: block;
    }
    
    .info {
        display: flex;
        align-items: center;
        gap: 6px;
        color: var(--text-color);
        text-decoration: none;
        padding: 12px 16px;
        flex: 1;
        
        .fileIcon {
            mask-image: url('../../../../public/icons/file.svg');
            mask-size: cover;
            mask-position: center;
            mask-repeat: no-repeat;
            
            width: 28px;
            height: 28px;
            margin: 2px;
            min-width: 28px;
            background-color: var(--text-color-secondary);
            opacity: 0.6;
        }

        .specificInfo {
            display: flex;
            gap: 4px;
            flex-direction: column;
            //width: clamp(150px, 25%, 250px);

            p {
                margin: 0;
                font-size: 16px;
                text-overflow: ellipsis;
                overflow: hidden;
                white-space: nowrap;
                padding-right: 8px;
                padding-bottom: 2px;
            }
            
            .fileDetails >p {
                font-size: 12px;
                color: var(--text-color-secondary);
            }
        }

        .favicon img {
            border-radius: 4px;
            overflow: hidden;
            width: 32px;
            height: 32px;
            min-width: 32px;
        }

        .description {
            margin-left: 10px;
            p {
                margin: 0;
                font-size: 14px;
                color: var(--text-color-secondary);
            }
        }
    }
    
    .editButtons {
        display: flex;
        gap: 8px;
        padding: 12px 16px;
    }
}

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