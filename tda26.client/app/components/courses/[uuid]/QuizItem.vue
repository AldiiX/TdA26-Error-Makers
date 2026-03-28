<script setup lang="ts">
import type {Quiz, Course} from "#shared/types";
import { NuxtLink } from '#components';
import ToggleVisibilityButton from "~/components/courses/[uuid]/ToggleVisibilityButton.vue";
import Popover from "~/components/Popover.vue";



const props = defineProps<{
    quiz: Quiz,
    course: Course,
    editMode?: boolean,
    isVisibilityToggleLoading?: boolean,
}>();

const emit = defineEmits<{
    (e: "edit", quiz: Quiz): void;
    (e: "delete", quiz: Quiz): void;
    (e: "toggleVisibility", quiz: Quiz): void;
    (e: "openResults", quiz: Quiz): void;
}>();

function toggleVisibility(): void {
    emit('toggleVisibility', props.quiz);
}
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
                <span
                    :class="[$style.modeBadge, quiz.mode === 'finaltest' ? $style.modeBadgeFinal : $style.modeBadgePractice]"
                >{{ quiz.mode === 'finaltest' ? 'Závěrečný test' : 'Procvičovací' }}</span>
            </div>
<!--                <p :class="$style.description">{{ quiz.description }}</p>-->
        </NuxtLink>
        
        <div v-if="editMode" :class="$style.editButtons">
            <button
                type="button"
                :class="[$style.iconButton, $style.iconButtonResults]"
                @click="emit('openResults', quiz)"
                title="Výsledky"
            >
                <span :class="$style.iconButtonIcon" aria-hidden="true"/>
            </button>
<!--            <ToggleVisibilityButton :is-visible="quiz.isVisible" :loading="isVisibilityToggleLoading" @toggle="toggleVisibility"/>-->
            <Popover teleport :disabled="course.status === 'draft'">
                <template #trigger>
                    <NuxtLink
                        :href="`/courses/${course.uuid}/quiz/${quiz.uuid}/edit`"
                        :disabled="course.status === 'draft'"
                    >
                        <button
                            type="button"
                            :class="[$style.iconButton, $style.iconButtonEdit]"
                            :disabled="course.status !== 'draft'"
                            title="Upravit"
                        >
                            <span :class="$style.iconButtonIcon" aria-hidden="true"/>
                        </button>
                    </NuxtLink>
                </template>

                <template #content>Kurz musí být návrh</template>
            </Popover>
            <Popover teleport :disabled="course.status === 'draft'">
                <template #trigger>
                    <button
                        type="button"
                        :class="[$style.iconButton, $style.iconButtonDelete]"
                        @click="emit('delete', quiz)"
                        :disabled="course.status !== 'draft'"
                        title="Smazat"
                    >
                        <span :class="$style.iconButtonIcon" aria-hidden="true"/>
                    </button>
                </template>

                <template #content>Kurz musí být návrh</template>
            </Popover>
        </div>

    </div>
    
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
        padding: 12px 16px;
        align-items: center;
        color: var(--text-color);
        text-decoration: none;
        flex: 1;

        &::before {
            content: "";
            width: 36px;     
            height: 36px;    
            flex: 0 0 36px;  
            display: block;
        }
        
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
            width: clamp(150px, 25%, 250px);
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
            margin-left: 24px;
            p {
                margin: 0;
                font-size: 14px;
                color: var(--text-color-secondary);
            }
        }
    }
    
    .editButtons {
        display: flex;
        gap: 4px;
        padding: 12px 16px;
    }
}

.iconButton {
    background: none;
    border: 1px solid transparent;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 6px;
    border-radius: 8px;
    transition: background-color 0.2s, border-color 0.2s;

    &:disabled {
        opacity: 0.35;
        cursor: not-allowed;
    }

    .iconButtonIcon {
        display: inline-block;
        width: 18px;
        height: 18px;
        min-width: 18px;
        mask-size: cover;
        mask-position: center;
        mask-repeat: no-repeat;
    }
}

.iconButtonResults {
    &:not(:disabled):hover {
        background-color: color-mix(in srgb, var(--accent-color-primary) 12%, transparent);
        border-color: color-mix(in srgb, var(--accent-color-primary) 30%, transparent);
    }

    .iconButtonIcon {
        mask-image: url('/icons/stats.svg');
        background-color: var(--accent-color-primary);
    }
}

.iconButtonEdit {
    &:not(:disabled):hover {
        background-color: color-mix(in srgb, var(--accent-color-secondary-theme) 12%, transparent);
        border-color: color-mix(in srgb, var(--accent-color-secondary-theme) 30%, transparent);
    }

    .iconButtonIcon {
        mask-image: url('/icons/pen.svg');
        background-color: var(--accent-color-secondary-theme, #2ecc71);
    }
}

.modeBadge {
    display: inline-block;
    font-size: 11px;
    font-weight: 600;
    padding: 2px 8px;
    border-radius: 999px;
    margin-top: 2px;
}

.modeBadgePractice {
    background-color: color-mix(in srgb, #27ae60 15%, transparent);
    color: #27ae60;
}

.modeBadgeFinal {
    background-color: color-mix(in srgb, #e74c3c 15%, transparent);
    color: #e74c3c;
}

.iconButtonDelete {
    &:not(:disabled):hover {
        background-color: color-mix(in srgb, #e74c3c 12%, transparent);
        border-color: color-mix(in srgb, #e74c3c 30%, transparent);
    }

    .iconButtonIcon {
        mask-image: url('/icons/trash.svg');
        background-color: #e74c3c;
    }
}
</style>