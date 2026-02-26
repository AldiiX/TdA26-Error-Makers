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
    (e: "itemDropped", itemUuid: string, itemType: "material" | "quiz", sourceModuleUuid?: string): void;
}>();

const isCollapsed = ref(false);
const isDraggingEnabled = computed(() => props.editMode === true && props.course.status === 'draft');

// ─── Drop-INTO-MODULE (flat item → module) ─────────────────────────────────
const isDragOver = ref(false);

// Reset isDragOver whenever any drag operation ends globally (handles cancelled drags / drops outside)
onMounted(() => document.addEventListener('dragend', resetDragOver));
onUnmounted(() => document.removeEventListener('dragend', resetDragOver));

function resetDragOver() {
    isDragOver.value = false;
    draggedItemUuid.value = null;
    dragOverItemUuid.value = null;
    dragOverEndZone.value = false;
}

function onDragOver(event: DragEvent) {
    if (!props.editMode || props.course.status !== 'draft') return;
    if (!event.dataTransfer?.types.includes(DRAG_ITEM_KEY)) return;
    event.preventDefault();
    event.stopPropagation();
    // We can't read the data during dragover (security restriction), so we show the overlay
    // and will guard against same-module drops in onDrop
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
    console.log('[CourseModuleSection onDrop]', props.module.title, '| raw:', raw, '| types:', event.dataTransfer?.types);
    if (!raw) return;
    event.preventDefault();
    event.stopPropagation();
    try {
        const { uuid, itemType, sourceModuleUuid } = JSON.parse(raw) as { uuid: string; itemType: 'material' | 'quiz'; sourceModuleUuid?: string };
        // Don't emit if item is being dropped onto the same module it came from
        if (sourceModuleUuid === props.module.uuid) return;
        emit('itemDropped', uuid, itemType, sourceModuleUuid);
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
// true when cursor is over the end-zone drop target (drop after last item)
const dragOverEndZone = ref(false);

function onItemDragStart(event: DragEvent, uuid: string) {
    draggedItemUuid.value = uuid;
    if (event.dataTransfer) {
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData(MODULE_ITEM_KEY, uuid);
        // Also set DRAG_ITEM_KEY so other module sections can detect this drag and show their drop overlay
        const item = moduleItems.value.find(i => i.uuid === uuid);
        if (item) {
            event.dataTransfer.setData(DRAG_ITEM_KEY, JSON.stringify({
                uuid,
                itemType: item.itemType,
                sourceModuleUuid: props.module.uuid,
            }));
        }
    }
    // stop propagation so the outer module-section drop target is not triggered
    event.stopPropagation();
}

function onItemDragOver(event: DragEvent, uuid: string) {
    // Only show within-module reorder hints when the drag originated from THIS module
    if (!draggedItemUuid.value) return;
    if (!event.dataTransfer?.types.includes(MODULE_ITEM_KEY)) return;
    event.preventDefault();
    event.stopPropagation();
    dragOverItemUuid.value = uuid;
    dragOverEndZone.value = false;
}

function onItemDragLeave(event: DragEvent, uuid: string) {
    const related = event.relatedTarget as Node | null;
    const current = event.currentTarget as HTMLElement | null;
    if (current && related && current.contains(related)) return;
    if (dragOverItemUuid.value === uuid) dragOverItemUuid.value = null;
}

function onEndZoneDragOver(event: DragEvent) {
    // Only show within-module end-zone hint when the drag originated from THIS module
    if (!draggedItemUuid.value) return;
    if (!event.dataTransfer?.types.includes(MODULE_ITEM_KEY)) return;
    event.preventDefault();
    event.stopPropagation();
    dragOverEndZone.value = true;
    dragOverItemUuid.value = null;
}

function onEndZoneDragLeave(event: DragEvent) {
    const related = event.relatedTarget as Node | null;
    const current = event.currentTarget as HTMLElement | null;
    if (current && related && current.contains(related)) return;
    dragOverEndZone.value = false;
}

async function performReorder(targetUuid: string | '__end__') {
    const dragged = draggedItemUuid.value;
    draggedItemUuid.value = null;

    if (!dragged) return;

    const items = [...moduleItems.value];
    const fromIdx = items.findIndex(i => i.uuid === dragged);
    if (fromIdx === -1) return;

    const [moved] = items.splice(fromIdx, 1);
    if (!moved) return;

    if (targetUuid === '__end__') {
        items.push(moved);
    } else {
        if (dragged === targetUuid) { items.splice(fromIdx, 0, moved); return; }
        const toIdx = items.findIndex(i => i.uuid === targetUuid);
        if (toIdx === -1) { items.splice(fromIdx, 0, moved); return; }
        items.splice(toIdx, 0, moved);
    }

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

async function onItemDrop(event: DragEvent, targetUuid: string) {
    // If this is a flat-item-to-module drop (not a within-module reorder), let it bubble up
    if (!event.dataTransfer?.types.includes(MODULE_ITEM_KEY)) return;

    // Check if the dragged item belongs to THIS module; if not, let the event bubble
    // up to the outer onDrop handler so it can handle the cross-module move.
    const draggedUuid = event.dataTransfer?.getData(MODULE_ITEM_KEY);
    if (draggedUuid && !moduleItems.value.some(i => i.uuid === draggedUuid)) return;

    event.preventDefault();
    event.stopPropagation();
    dragOverItemUuid.value = null;

    await performReorder(targetUuid);
}

async function onEndZoneDrop(event: DragEvent) {
    if (!event.dataTransfer?.types.includes(MODULE_ITEM_KEY)) return;

    // Check if the dragged item belongs to THIS module; if not, let the event bubble
    const draggedUuid = event.dataTransfer?.getData(MODULE_ITEM_KEY);
    if (draggedUuid && !moduleItems.value.some(i => i.uuid === draggedUuid)) return;
    event.preventDefault();
    event.stopPropagation();
    dragOverEndZone.value = false;

    await performReorder('__end__');
}

function onItemDragEnd() {
    draggedItemUuid.value = null;
    dragOverItemUuid.value = null;
    dragOverEndZone.value = false;
}
</script>

<template>
    <div
        :class="[$style.moduleSection, !module.isVisible && $style.hidden, isDragOver && !draggedItemUuid && $style.dropTarget]"
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

        <!-- Drop overlay hint (flat item → module, or item from another module) -->
        <div v-if="isDragOver && !draggedItemUuid" :class="$style.dropOverlay">
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
                            // [$style.itemDragOver]: dragOverItemUuid === item.uuid && draggedItemUuid !== item.uuid,
                        }"
                        @dragstart="isDraggingEnabled && onItemDragStart($event, item.uuid)"
                        @dragover="isDraggingEnabled && onItemDragOver($event, item.uuid)"
                        @dragleave="isDraggingEnabled && onItemDragLeave($event, item.uuid)"
                        @drop="isDraggingEnabled && onItemDrop($event, item.uuid)"
                        @dragend="onItemDragEnd"
                    >

                        <Transition name="drag-placeholder">
                            <div
                                v-if="isDraggingEnabled && dragOverItemUuid === item.uuid && draggedItemUuid !== item.uuid"
                                :class="$style.dragPlaceholder"
                            />
                        </Transition>
                        <div :class="$style.item">
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
                        </div>
                    </li>
                </ul>

                <!-- End-zone: drop target to place item after the last one -->
                <div
                    v-if="isDraggingEnabled && draggedItemUuid"
                    :class="[$style.endDropZone, dragOverEndZone && $style.endDropZoneActive]"
                    @dragover="onEndZoneDragOver"
                    @dragleave="onEndZoneDragLeave"
                    @drop="onEndZoneDrop"
                >
                    <Transition name="drag-placeholder">
                        <div v-if="dragOverEndZone" :class="$style.dragPlaceholder" />
                    </Transition>
                </div>

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
.dragPlaceholder {
    border: 2px dashed var(--accent-color-secondary-theme);
    background-color: rgb(from var(--accent-color-secondary-theme) r g b / 0.1);
    height: 63px;
    width: 100%;
    margin-bottom: 12px;
    border-radius: 12px;
    pointer-events: none;
    overflow: hidden;
}

.moduleSection {
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 15%, transparent);
    border-radius: 16px;
    overflow: hidden;
    transition: opacity 0.3s, border-color 0.2s, box-shadow 0.2s;
    position: relative;

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
    padding: 14px 32px 14px 16px;
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

.item {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 8px;
}

.itemRow {
    gap: 12px;
    border-radius: 10px;
    transition: opacity 0.2s, outline 0.15s;

    &.itemDragging {
        opacity: 0.35;
    }

    //&.itemDragOver {
    //    outline: 2px solid color-mix(in srgb, var(--accent-color-primary) 60%, transparent);
    //    outline-offset: 2px;
    //}
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

.endDropZone {
    min-height: 16px;
    width: 100%;
    border-radius: 10px;
    transition: min-height 0.2s;

    &.endDropZoneActive {
        min-height: 20px;
    }
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

<style lang="scss">
.drag-placeholder-enter-active,
.drag-placeholder-leave-active {
    transition: height 0.2s ease, margin-bottom 0.2s ease, opacity 0.2s ease;
}

.drag-placeholder-enter-from,
.drag-placeholder-leave-to {
    height: 0 !important;
    margin-bottom: 0 !important;
    opacity: 0;
}

.module-list-move {
    transition: transform 0.3s ease;
}
</style>