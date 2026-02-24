<script setup lang="ts">
import type {Quiz, QuizResultsSummary} from "#shared/types";
import Modal from "~/components/Modal.vue";
import { useCourseDialogs } from "~/composables/courses/[uuid]/useCourseDialogs";
import toClockTime from "~/utils/toClockTime";
import type {Course, CourseStatus} from "#shared/types";
import { NuxtLink } from '#components';
import Button from "~/components/Button.vue";
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
            </div>
<!--                <p :class="$style.description">{{ quiz.description }}</p>-->
        </NuxtLink>
        
        <div v-if="editMode" :class="$style.editButtons">
            <Button @click="emit('openResults', quiz)">Výsledky</Button>
            <ToggleVisibilityButton :is-visible="quiz.isVisible" :loading="isVisibilityToggleLoading" @toggle="toggleVisibility"/>
            <Popover teleport :disabled="course.status === 'draft'">
                <template #trigger>
                    <NuxtLink
                        :href="`/courses/${course.uuid}/quiz/${quiz.uuid}/edit`"
                        :disabled="course.status === 'draft'"
                    >
                        <Button
                            button-style="primary"
                            accent-color="secondary"
                            style="width: 100%"
                            :disabled="course.status !== 'draft'"
                        >Upravit</Button>
                    </NuxtLink>
                </template>

                <template #content>Kurz musí být návrh</template>
            </Popover>
            <Popover teleport :disabled="course.status === 'draft'">
                <template #trigger>
                    <Button
                        button-style="secondary"
                        accent-color="secondary"
                        style="width: 100%"
                        @click="emit('delete', quiz)"
                        :disabled="course.status !== 'draft'"
                    >Smazat</Button>
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
</style>