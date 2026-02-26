<script setup lang="ts">
import type { Course, Module, Material, Quiz } from "#shared/types";
import MaterialItem from "~/components/courses/[uuid]/MaterialItem.vue";
import QuizItem from "~/components/courses/[uuid]/QuizItem.vue";
import ToggleVisibilityButton from "~/components/courses/[uuid]/ToggleVisibilityButton.vue";
import Button from "~/components/Button.vue";
import Popover from "~/components/Popover.vue";
import { DRAG_ITEM_KEY } from "~/composables/courses/[uuid]/useModuleDrag";

const props = defineProps<{
    module: Module;
    course: Course;
    editMode?: boolean;
    isVisibilityToggleLoading?: boolean;
    itemLoadingStates?: Record<string, boolean>;
}>();

const emit = defineEmits<{
    (e: "editModule", module: Module): void;
    (e: "deleteModule", module: Module): void;
    (e: "toggleModuleVisibility", module: Module): void;
    (e: "editMaterial", material: Material): void;
    (e: "deleteMaterial", material: Material): void;
    (e: "toggleMaterialVisibility", material: Material): void;
    (e: "deleteQuiz", quiz: Quiz): void;
    (e: "toggleQuizVisibility", quiz: Quiz): void;
    (e: "openQuizResults", quiz: Quiz): void;
    (e: "addMaterial"): void;
    (e: "addQuiz"): void;
    (e: "itemDropped", itemUuid: string, itemType: "material" | "quiz"): void;
}>();

const isCollapsed = ref(false);
const isDraggingEnabled = computed(() => props.editMode === true && props.course.status === 'draft');

// ─── Drop-INTO-MODULE (flat item → module) ─────────────────────────────────
const isDragOver = ref(false);

function onDragOver(event: DragEvent) {
    if (!props.editMode || props.course.status !== 'draft') return;
    if (!event.dataTransfer?.types.includes(DRAG_ITEM_KEY)) return;
    event.preventDefault();
    event.stopPropagation();
    isDragOver.value = true;
}

function onDragLeave(event: DragEvent) {
    const related = event.relatedTarget as Node | null;
    const current = event.currentTarget as HTMLElement | null;
    if (current && related && current.contains(related)) return;
    isDragOver.value = false;
}

function onDrop(event: DragEvent) {
    isDragOver.value = false;
    if (!props.editMode || props.course.status !== 'draft') return;
    const raw = event.dataTransfer?.getData(DRAG_ITEM_KEY);
    if (!raw) return;
    event.preventDefault();
    event.stopPropagation();
    try {
        const { uuid, itemType } = JSON.parse(raw) as { uuid: string; itemType: 'material' | 'quiz' };
        emit('itemDropped', uuid, itemType);
    } catch {
        // ignore malformed data
    }
}

function onDragEnd() {
    isDragOver.value = false;
}

// ─── WITHIN-MODULE item reorder ─────────────────────────────────────────────
const MODULE_ITEM_KEY = 'application/module-item';

type ModuleItem =
    | (Material & { itemType: 'material' })
    | (Quiz    & { itemType: 'quiz' });

const moduleItems = computed<ModuleItem[]>(() => {
    const mats: ModuleItem[] = props.module.materials.map(m => ({ ...m, itemType: 'material' as const }));
    const qzs:  ModuleItem[] = props.module.quizzes.map(q => ({ ...q, itemType: 'quiz' as const }));
    return [...mats, ...qzs].sort((a, b) => a.order - b.order);
});

const draggedItemUuid = ref<string | null>(null);
const dragOverItemUuid = ref<string | null>(null);

function onItemDragStart(event: DragEvent, uuid: string) {
    draggedItemUuid.value = uuid;
    if (event.dataTransfer) {
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData(MODULE_ITEM_KEY, uuid);
    }
    // stop propagation so the outer module-section drop target is not triggered
    event.stopPropagation();
}

function onItemDragOver(event: DragEvent, uuid: string) {
    if (!event.dataTransfer?.types.includes(MODULE_ITEM_KEY)) return;
    event.preventDefault();
    event.stopPropagation();
    dragOverItemUuid.value = uuid;
}

function onItemDragLeave(event: DragEvent, uuid: string) {
    const related = event.relatedTarget as Node | null;
    const current = event.currentTarget as HTMLElement | null;
    if (current && related && current.contains(related)) return;
    if (dragOverItemUuid.value === uuid) dragOverItemUuid.value = null;
}

async function onItemDrop(event: DragEvent, targetUuid: string) {
    event.preventDefault();
    event.stopPropagation();
    dragOverItemUuid.value = null;

    const dragged = draggedItemUuid.value;
    draggedItemUuid.value = null;

    if (!dragged || dragged === targetUuid) return;

    const items = [...moduleItems.value];
    const fromIdx = items.findIndex(i => i.uuid === dragged);
    const toIdx   = items.findIndex(i => i.uuid === targetUuid);
    if (fromIdx === -1 || toIdx === -1) return;

    const [moved] = items.splice(fromIdx, 1);
    items.splice(toIdx, 0, moved);

    // Optimistically update order values on the source module arrays
    items.forEach((item, idx) => {
        if (item.itemType === 'material') {
            const mat = props.module.materials.find(m => m.uuid === item.uuid);
            if (mat) mat.order = idx;
        } else {
            const quiz = props.module.quizzes.find(q => q.uuid === item.uuid);
            if (quiz) quiz.order = idx;
        }
    });

    const newOrders = items.map((item, idx) => ({
        uuid: item.uuid,
        moduleType: item.itemType,
        order: idx,
    }));

    await $fetch(`/api/v1/courses/${props.course.uuid}/modules/${props.module.uuid}/items/reorder`, {
        method: 'POST',
        body: { moduleOrders: newOrders },
    }).catch(err => console.error('Error saving item order within module:', err));
}

function onItemDragEnd() {
    draggedItemUuid.value = null;
    dragOverItemUuid.value = null;
}
</script>

<template>
    <div
        :class="[$style.moduleSection, !module.isVisible && $style.hidden, isDragOver && $style.dropTarget]"
        @dragover="onDragOver"
        @dragleave="onDragLeave"
        @drop="onDrop"
        @dragend="onDragEnd"
    >
        <!-- Module header -->
        <div :class="$style.moduleHeader">
            <div :class="$style.moduleHeaderLeft">
                <button
                    :class="[$style.collapseButton, isCollapsed && $style.collapsed]"
                    type="button"
                    @click="isCollapsed = !isCollapsed"
                    :aria-label="isCollapsed ? 'Rozbalit modul' : 'Sbalit modul'"
                >
                    <svg width="12" height="12" viewBox="0 0 12 12" fill="currentColor">
                        <path d="M2 4l4 4 4-4" stroke="currentColor" stroke-width="1.5" fill="none" stroke-linecap="round" stroke-linejoin="round"/>
                    </svg>
                </button>
                <div :class="$style.moduleTitle">
                    <span>{{ module.title }}</span>
                    <span v-if="module.description" :class="$style.moduleDescription">{{ module.description }}</span>
                </div>
                <span v-if="!module.isVisible" :class="$style.hiddenBadge">Skryté</span>
            </div>

            <div v-if="editMode" :class="$style.moduleActions">
                <ToggleVisibilityButton
                    :is-visible="module.isVisible"
                    :loading="isVisibilityToggleLoading"
                    @toggle="emit('toggleModuleVisibility', module)"
                />
                <Popover teleport :disabled="course.status === 'draft'">
                    <template #trigger>
                        <Button
                            button-style="primary"
                            accent-color="secondary"
                            :disabled="course.status !== 'draft'"
                            @click="emit('editModule', module)"
                        >Upravit</Button>
                    </template>
                    <template #content>Kurz musí být návrh</template>
                </Popover>
                <Popover teleport :disabled="course.status === 'draft'">
                    <template #trigger>
                        <Button
                            button-style="secondary"
                            accent-color="secondary"
                            :disabled="course.status !== 'draft'"
                            @click="emit('deleteModule', module)"
                        >Smazat</Button>
                    </template>
                    <template #content>Kurz musí být návrh</template>
                </Popover>
            </div>
        </div>

        <!-- Drop overlay hint (flat item → module) -->
        <div v-if="isDragOver" :class="$style.dropOverlay">
            <p>Přidat do modulu „{{ module.title }}"</p>
        </div>

        <!-- Module items -->
        <Transition name="collapse">
            <div v-if="!isCollapsed" :class="$style.moduleItems">
                <p v-if="moduleItems.length === 0" :class="$style.emptyMessage">
                    Tento modul neobsahuje žádné materiály ani kvízy.
                </p>

                <ul v-else :class="$style.itemList">
                    <li
                        v-for="item in moduleItems"
                        :key="item.uuid"
                        :draggable="isDraggingEnabled"
                        :class="{
                            [$style.itemRow]: true,
                            [$style.itemDragging]: draggedItemUuid === item.uuid,
                            [$style.itemDragOver]: dragOverItemUuid === item.uuid && draggedItemUuid !== item.uuid,
                        }"
                        @dragstart="isDraggingEnabled && onItemDragStart($event, item.uuid)"
                        @dragover="isDraggingEnabled && onItemDragOver($event, item.uuid)"
                        @dragleave="isDraggingEnabled && onItemDragLeave($event, item.uuid)"
                        @drop="isDraggingEnabled && onItemDrop($event, item.uuid)"
                        @dragend="onItemDragEnd"
                    >
                        <div v-if="isDraggingEnabled" :class="$style.itemDragHandle">
                            <svg width="10" height="16" viewBox="0 0 10 16" fill="currentColor">
                                <circle cx="2" cy="2" r="1.5"/>
                                <circle cx="8" cy="2" r="1.5"/>
                                <circle cx="2" cy="8" r="1.5"/>
                                <circle cx="8" cy="8" r="1.5"/>
                                <circle cx="2" cy="14" r="1.5"/>
                                <circle cx="8" cy="14" r="1.5"/>
                            </svg>
                        </div>
                        <div :class="$style.itemContent">
                            <MaterialItem
                                v-if="item.itemType === 'material'"
                                :material="(item as Material)"
                                :course="course"
                                :edit-mode="editMode"
                                :is-visibility-toggle-loading="itemLoadingStates?.[item.uuid] ?? false"
                                @edit="emit('editMaterial', item as Material)"
                                @delete="emit('deleteMaterial', item as Material)"
                                @toggle-visibility="emit('toggleMaterialVisibility', item as Material)"
                            />
                            <QuizItem
                                v-else-if="item.itemType === 'quiz'"
                                :quiz="(item as Quiz)"
                                :course="course"
                                :edit-mode="editMode"
                                :is-visibility-toggle-loading="itemLoadingStates?.[item.uuid] ?? false"
                                @delete="emit('deleteQuiz', item as Quiz)"
                                @toggle-visibility="emit('toggleQuizVisibility', item as Quiz)"
                                @open-results="emit('openQuizResults', item as Quiz)"
                            />
                        </div>
                    </li>
                </ul>

                <!-- Add items buttons (edit mode only) -->
                <div v-if="editMode && course.status === 'draft'" :class="$style.addItemButtons">
                    <Button
                        button-style="tertiary"
                        accent-color="primary"
                        :class="$style.addItemButton"
                        @click="emit('addMaterial')"
                    >
                        + Přidat materiál
                    </Button>
                    <Button
                        button-style="tertiary"
                        accent-color="primary"
                        :class="$style.addItemButton"
                        @click="emit('addQuiz')"
                    >
                        + Přidat kvíz
                    </Button>
                </div>
            </div>
        </Transition>
    </div>
</template>

<style module lang="scss">
.moduleSection {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 15%, transparent);
    border-radius: 16px;
    overflow: hidden;
    transition: opacity 0.3s, border-color 0.2s, box-shadow 0.2s;
    position: relative;

    &.hidden {
        opacity: 0.6;
    }

    &.dropTarget {
        border-color: var(--accent-color-primary);
        box-shadow: 0 0 0 2px color-mix(in srgb, var(--accent-color-primary) 30%, transparent);
    }
}

.dropOverlay {
    position: absolute;
    inset: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    background-color: color-mix(in srgb, var(--accent-color-primary) 12%, transparent);
    border-radius: 16px;
    z-index: 10;
    pointer-events: none;

    p {
        margin: 0;
        font-size: 15px;
        font-weight: 600;
        color: var(--accent-color-primary);
        background-color: color-mix(in srgb, var(--background-color-secondary) 90%, transparent);
        padding: 8px 20px;
        border-radius: 999px;
        border: 1px solid color-mix(in srgb, var(--accent-color-primary) 40%, transparent);
    }
}

.moduleHeader {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 14px 16px;
    background-color: color-mix(in srgb, var(--background-color-secondary) 60%, transparent);
    gap: 12px;
    flex-wrap: wrap;
}

.moduleHeaderLeft {
    display: flex;
    align-items: center;
    gap: 10px;
    flex: 1;
    min-width: 0;
}

.collapseButton {
    background: none;
    border: none;
    cursor: pointer;
    color: var(--text-color-secondary);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 4px;
    border-radius: 4px;
    transition: transform 0.2s, background-color 0.2s;
    flex-shrink: 0;

    &:hover {
        background-color: color-mix(in srgb, var(--text-color-secondary) 10%, transparent);
    }

    &.collapsed {
        transform: rotate(-90deg);
    }
}

.moduleTitle {
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;

    > span:first-child {
        font-size: 16px;
        font-weight: 600;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
}

.moduleDescription {
    font-size: 13px;
    color: var(--text-color-secondary);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.hiddenBadge {
    font-size: 11px;
    font-weight: 600;
    padding: 2px 8px;
    border-radius: 999px;
    background-color: color-mix(in srgb, var(--color-warning, orange) 15%, transparent);
    color: var(--color-warning, orange);
    white-space: nowrap;
    flex-shrink: 0;
}

.moduleActions {
    display: flex;
    gap: 8px;
    flex-shrink: 0;
    flex-wrap: wrap;
}

.moduleItems {
    display: flex;
    flex-direction: column;
    gap: 8px;
    padding: 12px 16px 16px;
}

.emptyMessage {
    margin: 0;
    font-size: 14px;
    color: var(--text-color-secondary);
    text-align: center;
    padding: 12px 0;
}

.itemList {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.itemRow {
    display: flex;
    align-items: center;
    gap: 6px;
    border-radius: 10px;
    transition: opacity 0.2s, outline 0.15s;

    &.itemDragging {
        opacity: 0.35;
    }

    &.itemDragOver {
        outline: 2px solid color-mix(in srgb, var(--accent-color-primary) 60%, transparent);
        outline-offset: 2px;
    }
}

.itemDragHandle {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    min-width: 18px;
    height: 36px;
    cursor: grab;
    color: var(--text-color-secondary);
    opacity: 0.4;
    flex-shrink: 0;
    transition: opacity 0.2s;

    &:hover {
        opacity: 0.8;
    }

    &:active {
        cursor: grabbing;
    }
}

.itemContent {
    flex: 1;
    min-width: 0;
}

.addItemButtons {
    display: flex;
    gap: 8px;
    margin-top: 4px;
    flex-wrap: wrap;
}

.addItemButton {
    font-size: 14px;
}
</style>

<style scoped>
.collapse-enter-active,
.collapse-leave-active {
    transition: opacity 0.2s ease;
}
.collapse-enter-from,
.collapse-leave-to {
    opacity: 0;
}
</style>

