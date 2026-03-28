<script setup lang="ts">
import { ref, computed } from "vue";
import Modal from "~/components/Modal.vue";

interface DailyQuestModalProps {
    enabled: boolean;
}

const props = defineProps<DailyQuestModalProps>();

const emit = defineEmits<{
    (e: "close"): void;
}>();

// Calendar state
const today = new Date();
const viewDate = ref(new Date(today.getFullYear(), today.getMonth(), 1));

const MONTH_NAMES = [
    "Leden", "Únor", "Březen", "Duben", "Květen", "Červen",
    "Červenec", "Srpen", "Září", "Říjen", "Listopad", "Prosinec"
];

const DAY_NAMES = ["Po", "Út", "St", "Čt", "Pá", "So", "Ne"];

const currentMonthLabel = computed(() => {
    return `${MONTH_NAMES[viewDate.value.getMonth()]} ${viewDate.value.getFullYear()}`;
});

// Returns an array of day cells for the calendar grid (including leading/trailing empty cells)
const calendarDays = computed(() => {
    const year = viewDate.value.getFullYear();
    const month = viewDate.value.getMonth();

    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);

    // getDay(): 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    // Shift so Monday = 0, ..., Sunday = 6
    const startOffset = (firstDay.getDay() + 6) % 7;
    const daysInMonth = lastDay.getDate();

    const cells: { day: number | null; isToday: boolean }[] = [];

    for (let i = 0; i < startOffset; i++) {
        cells.push({ day: null, isToday: false });
    }

    for (let d = 1; d <= daysInMonth; d++) {
        const isToday =
            d === today.getDate() &&
            month === today.getMonth() &&
            year === today.getFullYear();
        cells.push({ day: d, isToday });
    }

    return cells;
});

const prevMonth = () => {
    const d = viewDate.value;
    viewDate.value = new Date(d.getFullYear(), d.getMonth() - 1, 1);
};

const nextMonth = () => {
    const d = viewDate.value;
    viewDate.value = new Date(d.getFullYear(), d.getMonth() + 1, 1);
};

// Placeholder daily quests
const quests = ref([
    { id: 1, title: "Dokončit 1 lekci", description: "Projdi alespoň jednu lekci v kurzu", done: false },
    { id: 2, title: "Přečíst studijní materiál", description: "Otevři a přečti jeden studijní materiál", done: false },
    { id: 3, title: "Vyřešit kvíz", description: "Dokonči alespoň jeden kvíz", done: false },
    { id: 4, title: "Přidat nový kurz", description: "Vytvoř nebo aktualizuj svůj kurz", done: false },
]);

const toggleQuest = (id: number) => {
    const quest = quests.value.find((q) => q.id === id);
    if (quest) quest.done = !quest.done;
};
</script>

<template>
    <Modal
        :enabled="props.enabled"
        :modal-style="{ maxWidth: '560px', width: '95vw', padding: '28px' }"
        @close="emit('close')"
    >
        <div :class="$style.wrap">
            <!-- Calendar header -->
            <div :class="$style.calHeader">
                <button :class="$style.navBtn" @click="prevMonth" aria-label="Předchozí měsíc">
                    <span :class="$style.arrowLeft" />
                </button>
                <h2 :class="$style.monthLabel">{{ currentMonthLabel }}</h2>
                <button :class="$style.navBtn" @click="nextMonth" aria-label="Příští měsíc">
                    <span :class="$style.arrowRight" />
                </button>
            </div>

            <!-- Day-of-week headers -->
            <div :class="$style.calGrid">
                <div
                    v-for="day in DAY_NAMES"
                    :key="day"
                    :class="[$style.calCell, $style.dayName]"
                >
                    {{ day }}
                </div>

                <!-- Day cells -->
                <div
                    v-for="(cell, idx) in calendarDays"
                    :key="idx"
                    :class="[
                        $style.calCell,
                        $style.dayCell,
                        cell.day === null && $style.empty,
                        cell.isToday && $style.today,
                    ]"
                >
                    <span v-if="cell.day !== null">{{ cell.day }}</span>
                </div>
            </div>

            <!-- Divider -->
            <div :class="$style.divider" />

            <!-- Daily quests section -->
            <div :class="$style.questsSection">
                <h3 :class="$style.questsTitle">Dnešní úkoly</h3>

                <ul :class="$style.questList">
                    <li
                        v-for="quest in quests"
                        :key="quest.id"
                        :class="[$style.questItem, quest.done && $style.questDone]"
                        @click="toggleQuest(quest.id)"
                    >
                        <div :class="[$style.checkbox, quest.done && $style.checkboxDone]">
                            <span v-if="quest.done" :class="$style.checkmark" />
                        </div>
                        <div :class="$style.questText">
                            <p :class="$style.questName">{{ quest.title }}</p>
                            <p :class="$style.questDesc">{{ quest.description }}</p>
                        </div>
                    </li>
                </ul>
            </div>
        </div>
    </Modal>
</template>

<style module lang="scss">
.wrap {
    display: flex;
    flex-direction: column;
    gap: 20px;
    padding-top: 8px;
}

/* ── Calendar header ── */
.calHeader {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 8px;
}

.monthLabel {
    margin: 0;
    font-size: 22px;
    font-weight: 700;
    text-align: center;
    flex: 1;
}

.navBtn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border: none;
    border-radius: 50%;
    background-color: var(--background-color-3);
    cursor: pointer;
    transition: background-color 0.2s, filter 0.2s;
    flex-shrink: 0;

    &:hover {
        filter: brightness(0.85);
    }
}

.arrowLeft,
.arrowRight {
    display: block;
    width: 14px;
    height: 14px;
    background-color: var(--text-color-primary);
    mask-image: url("/icons/arrow.svg");
    mask-size: contain;
    mask-repeat: no-repeat;
    mask-position: center;
}

.arrowLeft {
    transform: rotate(90deg);
}

.arrowRight {
    transform: rotate(-90deg);
}

/* ── Calendar grid ── */
.calGrid {
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    gap: 4px;
}

.calCell {
    display: flex;
    align-items: center;
    justify-content: center;
    aspect-ratio: 1 / 1;
    border-radius: 50%;
    font-size: 14px;
    user-select: none;
}

.dayName {
    font-weight: 700;
    font-size: 12px;
    color: var(--text-color-secondary);
    aspect-ratio: unset;
    padding-bottom: 2px;
}

.dayCell {
    cursor: default;
    transition: background-color 0.15s;

    &:not(.empty):hover {
        background-color: var(--accent-color-primary-transparent-01);
    }
}

.empty {
    opacity: 0;
    pointer-events: none;
}

.today {
    background-color: var(--accent-color-primary) !important;
    color: var(--accent-color-primary-text) !important;
    font-weight: 700;
}

/* ── Divider ── */
.divider {
    height: 1px;
    background-color: var(--background-color-3);
    border-radius: 999px;
}

/* ── Daily quests ── */
.questsSection {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.questsTitle {
    margin: 0;
    font-size: 18px;
    font-weight: 700;
}

.questList {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.questItem {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 12px 16px;
    border-radius: 12px;
    background-color: var(--background-color-3);
    cursor: pointer;
    transition: background-color 0.2s, opacity 0.2s;
    user-select: none;

    &:hover {
        background-color: var(--accent-color-primary-transparent-01);
    }
}

.questDone {
    opacity: 0.55;
}

.checkbox {
    width: 22px;
    height: 22px;
    border-radius: 6px;
    border: 2px solid var(--accent-color-primary);
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background-color 0.2s;
}

.checkboxDone {
    background-color: var(--accent-color-primary);
}

.checkmark {
    display: block;
    width: 12px;
    height: 12px;
    background-color: var(--accent-color-primary-text);
    mask-image: url("/icons/done.svg");
    mask-size: contain;
    mask-repeat: no-repeat;
    mask-position: center;
}

.questText {
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;
}

.questName {
    margin: 0;
    font-size: 15px;
    font-weight: 600;
}

.questDesc {
    margin: 0;
    font-size: 13px;
    color: var(--text-color-secondary);
}
</style>
