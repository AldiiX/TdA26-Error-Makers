<script setup lang="ts">
import type { Course, Module, Material, Quiz } from "#shared/types";
import MaterialItem from "~/components/courses/[uuid]/MaterialItem.vue";
import QuizItem from "~/components/courses/[uuid]/QuizItem.vue";
import ToggleVisibilityButton from "~/components/courses/[uuid]/ToggleVisibilityButton.vue";
import Button from "~/components/Button.vue";
import Popover from "~/components/Popover.vue";

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
}>();

const isCollapsed = ref(false);
</script>

<template>
    <div :class="[$style.moduleSection, !module.isVisible && $style.hidden]">
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

        <!-- Module items -->
        <Transition name="collapse">
            <div v-if="!isCollapsed" :class="$style.moduleItems">
                <p v-if="module.materials.length === 0 && module.quizzes.length === 0" :class="$style.emptyMessage">
                    Tento modul neobsahuje žádné materiály ani kvízy.
                </p>

                <template v-for="material in [...module.materials].sort((a,b) => a.order - b.order)" :key="material.uuid">
                    <MaterialItem
                        :material="material"
                        :course="course"
                        :edit-mode="editMode"
                        :is-visibility-toggle-loading="itemLoadingStates?.[material.uuid] ?? false"
                        @edit="emit('editMaterial', material)"
                        @delete="emit('deleteMaterial', material)"
                        @toggle-visibility="emit('toggleMaterialVisibility', material)"
                    />
                </template>

                <template v-for="quiz in [...module.quizzes].sort((a,b) => a.order - b.order)" :key="quiz.uuid">
                    <QuizItem
                        :quiz="quiz"
                        :course="course"
                        :edit-mode="editMode"
                        :is-visibility-toggle-loading="itemLoadingStates?.[quiz.uuid] ?? false"
                        @delete="emit('deleteQuiz', quiz)"
                        @toggle-visibility="emit('toggleQuizVisibility', quiz)"
                        @open-results="emit('openQuizResults', quiz)"
                    />
                </template>

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
    transition: opacity 0.3s;

    &.hidden {
        opacity: 0.6;
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
