<script setup lang="ts">
import Modal from "~/components/Modal.vue";
import Button from "~/components/Button.vue";
import useDailyRewards from "~/composables/useDailyRewards";
import type { DailyRewardDay } from "#shared/types";

const props = defineProps<{
    enabled: boolean;
}>();

const emit = defineEmits<{
    (e: "close"): void;
}>();

const { monthData, loading, claiming, error, fetchMonth, claimDay } = useDailyRewards();

const selectedMonth = ref(new Date());
const selectedDate = ref<string>(toDateKey(new Date()));

const visibleYear = computed(() => selectedMonth.value.getFullYear());
const visibleMonth = computed(() => selectedMonth.value.getMonth() + 1);

const weekdayLabels = ["Po", "Ut", "St", "Ct", "Pa", "So", "Ne"];

const monthLabel = computed(() =>
    new Intl.DateTimeFormat("cs-CZ", { month: "long", year: "numeric" }).format(selectedMonth.value)
);

const dayMap = computed(() => {
    const map = new Map<string, DailyRewardDay>();
    for (const day of monthData.value?.days ?? []) {
        map.set(day.date, day);
    }
    return map;
});

const calendarCells = computed(() => {
    const year = visibleYear.value;
    const month = visibleMonth.value;
    const daysInMonth = monthData.value?.daysInMonth ?? new Date(year, month, 0).getDate();
    const firstWeekDay = new Date(year, month - 1, 1).getDay();
    const mondayOffset = (firstWeekDay + 6) % 7;

    const cells: Array<null | DailyRewardDay> = [];
    for (let i = 0; i < mondayOffset; i++) {
        cells.push(null);
    }

    for (let day = 1; day <= daysInMonth; day++) {
        const dateKey = toDateKey(new Date(year, month - 1, day));
        cells.push(dayMap.value.get(dateKey) ?? {
            date: dateKey,
            isClaimed: false,
            claimedAt: null,
            canClaim: false,
            isCompleted: false,
            tasks: []
        });
    }

    return cells;
});

const selectedDay = computed(() => dayMap.value.get(selectedDate.value) ?? null);

const open = async () => {
    await fetchMonth(visibleYear.value, visibleMonth.value);

    if (!dayMap.value.has(selectedDate.value)) {
        selectedDate.value = toDateKey(new Date(visibleYear.value, visibleMonth.value - 1, 1));
    }
};

const goToPreviousMonth = async () => {
    await setMonthAndSelectDay(new Date(visibleYear.value, visibleMonth.value - 2, 1), 1);
};

const goToNextMonth = async () => {
    await setMonthAndSelectDay(new Date(visibleYear.value, visibleMonth.value, 1), 1);
};

const goToToday = async () => {
    const now = new Date();
    await setMonthAndSelectDay(now, now.getDate());
};

const setMonthAndSelectDay = async (date: Date, day: number) => {
    selectedMonth.value = new Date(date.getFullYear(), date.getMonth(), 1);
    selectedDate.value = toDateKey(new Date(date.getFullYear(), date.getMonth(), Math.max(1, day)));
    await fetchMonth(visibleYear.value, visibleMonth.value);
};

const handleClaim = async () => {
    if (!selectedDay.value || !selectedDay.value.canClaim) return;

    const claimedDay = await claimDay(selectedDay.value.date);
    if (!claimedDay) return;

    await fetchMonth(visibleYear.value, visibleMonth.value);
    selectedDate.value = claimedDay.date;
};

watch(
    () => props.enabled,
    async (enabled) => {
        if (!enabled) return;
        await open();
    },
    { immediate: true }
);

function toDateKey(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, "0");
    const day = `${date.getDate()}`.padStart(2, "0");
    return `${year}-${month}-${day}`;
}
</script>

<template>
    <Modal
        :enabled="props.enabled"
        :can-be-closed-by-clicking-outside="true"
        :modal-style="{ maxWidth: '980px', width: '92vw' }"
        @close="emit('close')"
    >
        <div :class="$style.wrapper">
            <div :class="$style.header">
                <h2>Daily rewards</h2>
                <div :class="$style.monthControls">
                    <Button button-style="secondary" @click="goToPreviousMonth">Predchozi</Button>
                    <p>{{ monthLabel }}</p>
                    <Button button-style="secondary" @click="goToNextMonth">Dalsi</Button>
                </div>
                <Button button-style="tertiary" @click="goToToday">Dnes</Button>
            </div>

            <p v-if="error" :class="$style.error">{{ error }}</p>

            <div :class="$style.content">
                <div :class="$style.calendarSection">
                    <div :class="$style.weekdays">
                        <p v-for="weekday in weekdayLabels" :key="weekday">{{ weekday }}</p>
                    </div>

                    <div :class="$style.calendarGrid">
                        <template v-for="(cell, index) in calendarCells" :key="`${index}-${cell?.date ?? 'empty'}`">
                            <div v-if="!cell" :class="$style.emptyCell" />

                            <button
                                v-else
                                :class="[
                                    $style.dayCell,
                                    selectedDate === cell.date && $style.selected,
                                    cell.isClaimed && $style.claimed,
                                    cell.canClaim && $style.readyToClaim,
                                    cell.isCompleted && $style.completedDay
                                ]"
                                type="button"
                                @click="selectedDate = cell.date"
                            >
                                <span v-if="cell.isCompleted" :class="$style.dayCheckmark">✓</span>
                                <span>{{ Number(cell.date.split('-')[2]) }}</span>
                                <small>
                                    <template v-if="cell.isClaimed">Claimed</template>
                                    <template v-else-if="cell.isCompleted">Done</template>
                                    <template v-else-if="cell.canClaim">Ready</template>
                                    <template v-else>Open</template>
                                </small>
                            </button>
                        </template>
                    </div>
                </div>

                <div :class="$style.taskSection">
                    <h3 v-if="selectedDay">Ukoly pro {{ selectedDay.date }}</h3>
                    <h3 v-else>Vyber den</h3>

                    <div v-if="loading" :class="$style.stateInfo">Nacitam daily rewards...</div>

                    <template v-else-if="selectedDay">
                        <div
                            v-for="task in selectedDay.tasks"
                            :key="task.taskCode"
                            :class="[$style.taskCard, task.isCompleted && $style.taskDone]"
                        >
                            <div>
                                <p :class="$style.taskTitle">{{ task.title }}</p>
                                <p :class="$style.taskDescription">{{ task.description }}</p>
                                <p :class="$style.taskRewards">+{{ task.rewardXp }} XP • +{{ task.rewardDuck }} Duck</p>
                            </div>
                            <p :class="$style.taskProgress">
                                {{ task.currentValue }} / {{ task.targetValue }}
                            </p>
                        </div>

                        <div v-if="selectedDay.tasks.length === 0" :class="$style.stateInfo">
                            Tento den zatim nema vytvorenou aktivitu.
                        </div>

                        <Button
                            v-if="!selectedDay.isClaimed"
                            button-style="primary"
                            :disabled="!selectedDay.canClaim || claiming"
                            @click="handleClaim"
                        >
                            {{ claiming ? 'Odemykam...' : 'Odemknout odmenu' }}
                        </Button>

                        <p v-else :class="$style.claimedInfo">Odmena uz byla odemknuta.</p>
                    </template>
                </div>
            </div>
        </div>
    </Modal>
</template>

<style module lang="scss">
.wrapper {
    display: flex;
    flex-direction: column;
    gap: 16px;
    color: var(--text-color-primary);
}

.header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 16px;
    gap: 8px;

    h2 {
        margin: 0;
    }
}

.monthControls {
    display: flex;
    align-items: center;
    gap: 12px;

    p {
        margin: 0;
        min-width: 180px;
        text-align: center;
        text-transform: capitalize;
        font-weight: 700;
    }
}

.error {
    margin: 0;
    color: rgb(220, 38, 38);
}

.content {
    display: grid;
    grid-template-columns: 1.15fr 1fr;
    gap: 16px;
}

.calendarSection {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.weekdays {
    display: grid;
    grid-template-columns: repeat(7, minmax(0, 1fr));
    gap: 8px;

    p {
        margin: 0;
        text-align: center;
        color: var(--text-color-secondary);
        font-weight: 700;
    }
}

.calendarGrid {
    display: grid;
    grid-template-columns: repeat(7, minmax(0, 1fr));
    gap: 8px;
}

.emptyCell {
    min-height: 64px;
}

.dayCell {
    position: relative;
    min-height: 64px;
    border-radius: 10px;
    border: 1px solid rgb(from var(--text-color-primary) r g b / 0.18);
    background: var(--background-color-primary);
    color: var(--text-color-primary);
    cursor: pointer;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    gap: 4px;

    span {
        font-size: 18px;
        font-weight: 700;
        line-height: 1;
    }

    small {
        font-size: 11px;
        color: var(--text-color-secondary);
    }

    &:hover {
        border-color: var(--accent-color-primary);
    }

    &.selected {
        border-color: var(--accent-color-primary);
        box-shadow: 0 0 0 1px var(--accent-color-primary) inset;
    }

    &.claimed {
        background: rgb(from var(--accent-color-secondary-theme) r g b / 0.15);
    }

    &.readyToClaim {
        border-color: var(--accent-color-secondary-theme);
    }

    &.completedDay {
        border-color: rgb(from var(--accent-color-secondary-theme) r g b / 0.9);
        box-shadow: 0 0 0 1px rgb(from var(--accent-color-secondary-theme) r g b / 0.6) inset;
    }
}

.dayCheckmark {
    position: absolute;
    top: 6px;
    right: 8px;
    color: rgb(from var(--accent-color-secondary-theme) r g b / 1);
    font-size: 14px;
    line-height: 1;
    font-weight: 900;
}

.taskSection {
    display: flex;
    flex-direction: column;
    gap: 12px;
    margin-top: 8px;
    padding-top: 8px;

    h3 {
        margin: 0;
    }
}

.taskCard {
    border: 1px solid rgb(from var(--text-color-primary) r g b / 0.14);
    border-radius: 12px;
    padding: 12px;
    display: flex;
    justify-content: space-between;
    align-items: start;
    gap: 12px;

    &.taskDone {
        border-color: rgb(from var(--accent-color-secondary-theme) r g b / 0.6);
        background: rgb(from var(--accent-color-secondary-theme) r g b / 0.08);
    }
}

.taskTitle {
    margin: 0;
    font-weight: 700;
}

.taskDescription {
    margin: 6px 0 0;
    color: var(--text-color-secondary);
}

.taskRewards {
    margin: 8px 0 0;
    color: var(--text-color-secondary);
    font-size: 13px;
    font-weight: 700;
}

.taskProgress {
    margin: 0;
    font-weight: 700;
}

.stateInfo {
    margin: 0;
    color: var(--text-color-secondary);
}

.claimedInfo {
    margin: 0;
    color: rgb(from var(--accent-color-secondary-theme) r g b / 0.95);
    font-weight: 700;
}

@media (max-width: 980px) {
    .content {
        grid-template-columns: 1fr;
    }

    .header {
        flex-wrap: wrap;
        align-items: stretch;
    }

    .monthControls {
        width: 100%;
        justify-content: space-between;

        p {
            min-width: unset;
            flex: 1;
        }
    }

    .taskSection {
        margin-top: 4px;
        padding-top: 4px;
    }

    .taskCard {
        flex-direction: column;
        align-items: flex-start;
    }
}
</style>


