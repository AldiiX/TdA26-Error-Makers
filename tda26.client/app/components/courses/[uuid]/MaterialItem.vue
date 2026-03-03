<script setup lang="ts">
import type {Course, gRecaptcha, Material} from "#shared/types";
import { NuxtLink } from '#components';
import ToggleVisibilityButton from "~/components/courses/[uuid]/ToggleVisibilityButton.vue";
import Popover from "~/components/Popover.vue";

const props = defineProps<{
    material: Material,
    course: Course,
    editMode?: boolean,
    isVisibilityToggleLoading?: boolean,
}>();

declare const grecaptcha: gRecaptcha;

const onUrlClick = async (e: MouseEvent) => {
    window.open(props.material.url!, "_blank", "noopener,noreferrer");
    grecaptcha.ready(async () => {
        const recaptchaToken = await grecaptcha.execute(
            "6LfDQhksAAAAAEz_ujbJNian3-e-TfyKx8gzRaCL",
            { action: "submit" }
        );

        const url = `/api/v1/courses/${props.course.uuid}/materials/${props.material.uuid}/track-click`;

        await fetch(url, { method: "POST", body: JSON.stringify({ recaptchaToken }), headers: { "Content-Type": "application/json"} }).catch(() => {});
    })
};

const emit = defineEmits<{
    (e: "edit", material: Material): void;
    (e: "delete", material: Material): void;
    (e: "toggleVisibility", material: Material): void;
    (e: "openMaterialResults", material: Material): void;
}>();

const getHostname = (url?: string) => {
    try {
        return url ? new URL(url).hostname : ''
    } catch {
        return ''
    }
}

function toggleVisibility(): void {
    emit('toggleVisibility', props.material);
}
</script>

<template>
    <!-- FILE MATERIAL -->
    <template v-if="material.type === 'file'">
        <div :class="$style.material">
            <NuxtLink :href="`/api/v1/courses/${course.uuid}/materials/${material.uuid}`" :class="$style.info" target="_blank" rel="noopener noreferrer">
                <div :class="$style.fileIcon"/>

                <div :class="$style.fileInfo">
                    <p :title="material.name">{{ material.name }}</p>
                    <div :class="$style.fileDetails">
                        <p>{{ material.fileUrl?.match(/\.([^.]+)$/)?.[1]?.toUpperCase() ?? "JINÉ" }} • {{ new Date(material.createdAt).toLocaleDateString() }}</p>
                    </div>
                </div>
                <p :class="$style.description">{{ material.description }}</p>
            </NuxtLink>
            
            <div v-if="editMode" :class="$style.editButtons">
                <button
                    type="button"
                    :class="[$style.iconButton, $style.iconButtonResults]"
                    @click="emit('openMaterialResults', material)"
                    title="Výsledky"
                >
                    <span :class="$style.iconButtonIcon" aria-hidden="true"/>
                </button>
<!--                <ToggleVisibilityButton :is-visible="material.isVisible" :loading="isVisibilityToggleLoading" @toggle="toggleVisibility"/>-->
                <Popover teleport :disabled="course.status === 'draft'">
                    <template #trigger>
                        <button
                            type="button"
                            :class="[$style.iconButton, $style.iconButtonEdit]"
                            @click="emit('edit', material)"
                            :disabled="course.status !== 'draft'"
                            title="Upravit"
                        >
                            <span :class="$style.iconButtonIcon" aria-hidden="true"/>
                        </button>
                    </template>

                    <template #content>Kurz musí být návrh</template>
                </Popover>
                <Popover teleport :disabled="course.status === 'draft'">
                    <template #trigger>
                        <button
                            type="button"
                            :class="[$style.iconButton, $style.iconButtonDelete]"
                            @click="emit('delete', material)"
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

    <!-- URL MATERIAL -->
    <template v-else-if="material.type === 'url'">
        <div :class="$style.material">
            <NuxtLink :href="material.url" :class="$style.info" target="_blank" rel="noopener noreferrer" @click.prevent="onUrlClick">
                <div :class="$style.favicon">
                    <img v-if="material.faviconUrl" :src="material.faviconUrl" alt="Favicon" >
                </div>

                <div :class="$style.fileInfo">
                    <p :title="material.name">{{ material.name }}</p>
                    <div :class="$style.fileDetails">
                        <p>{{ getHostname(material.url) }} • {{ new Date(material.createdAt).toLocaleDateString() }}</p>
                    </div>
                </div>
                <p :class="$style.description">{{ material.description }}</p>
            </NuxtLink>
            
            <div v-if="editMode" :class="$style.editButtons">
                <button
                    type="button"
                    :class="[$style.iconButton, $style.iconButtonResults]"
                    @click="emit('openMaterialResults', material)"
                    title="Výsledky"
                >
                    <span :class="$style.iconButtonIcon" aria-hidden="true"/>
                </button>
<!--                <ToggleVisibilityButton :is-visible="material.isVisible" :loading="isVisibilityToggleLoading" @toggle="toggleVisibility"/>-->
                <Popover teleport :disabled="course.status === 'draft'">
                    <template #trigger>
                        <button
                            type="button"
                            :class="[$style.iconButton, $style.iconButtonEdit]"
                            @click="emit('edit', material)"
                            :disabled="course.status !== 'draft'"
                            title="Upravit"
                        >
                            <span :class="$style.iconButtonIcon" aria-hidden="true"/>
                        </button>
                    </template>

                    <template #content>Kurz musí být návrh</template>
                </Popover>
                <Popover teleport :disabled="course.status === 'draft'">
                    <template #trigger>
                        <button
                            type="button"
                            :class="[$style.iconButton, $style.iconButtonDelete]"
                            @click="emit('delete', material)"
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
</template>

<style module lang="scss">
.material {
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

    .info {
        display: flex;
        align-items: center;
        gap: 6px;
        color: var(--text-color);
        text-decoration: none;
        padding: 12px 16px;
        flex: 1;
        
        .fileIcon {
            mask-image: url('/icons/file.svg');
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

        .fileInfo {
            display: flex;
            flex-direction: column;
            gap: 4px;
            border-right: 1px solid color-mix(in srgb, var(--text-color-secondary) 20%, transparent 40%);
            width: clamp(150px, 25%, 250px);

            p {
                margin: 0;
                font-size: 16px;
                text-overflow: ellipsis;
                overflow: hidden;
                white-space: nowrap;
                padding-right: 8px;
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
            height: 100%;
            font-size: 14px;
            color: var(--text-color-secondary);
            display: -webkit-box;
            
            //-webkit-line-clamp: 3;
            //-webkit-box-orient: vertical;
            //
            //overflow: hidden;
            //text-overflow: ellipsis;
            //line-height: 1.2;
            //max-height: calc(1.2em * 3);
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