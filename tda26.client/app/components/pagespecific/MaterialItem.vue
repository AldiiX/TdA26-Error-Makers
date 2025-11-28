<script setup lang="ts">
import type {Material} from "#shared/types";
import { NuxtLink } from '#components';
import Button from "~/components/Button.vue";

const props = defineProps<{
    material: Material,
    course: { uuid: string },
    editMode?: boolean
}>();

const emit = defineEmits<{
    (e: "edit", material: Material): void;
    (e: "delete", material: Material): void;
}>();

const getHostname = (url?: string) => {
    try {
        return url ? new URL(url).hostname : ''
    } catch {
        return ''
    }
}
</script>

<template>
    <!-- FILE MATERIAL -->
    <template v-if="material.type === 'file'">
        <div :class="$style.material">
            <NuxtLink :href="`/api/v2/courses/${course.uuid}/materials/${material.uuid}`" :class="$style.info" target="_blank" rel="noopener noreferrer">
                <div :class="$style.fileIcon"></div>

                <div :class="$style.fileInfo">
                    <p>{{ material.name }}</p>
                    <div :class="$style.fileDetails">
                        <p>{{ material.fileUrl?.match(/\.([^.]+)$/)?.[1]?.toUpperCase() ?? "JINÉ" }} • {{ new Date(material.createdAt).toLocaleDateString() }}</p>
                    </div>
                </div>
                <p :class="$style.description">{{ material.description }}</p>
            </NuxtLink>
            
            <div v-if="editMode" :class="$style.editButtons">
                <Button button-style="primary" accent-color="secondary" @click="emit('edit', material)" style="width: 100%">Upravit</Button>
                <Button button-style="secondary" accent-color="secondary" @click="emit('delete', material)" style="width: 100%">Smazat</Button>
            </div>
        </div>
    </template>

    <!-- URL MATERIAL -->
    <template v-else-if="material.type === 'url'">
        <div :class="$style.material">
            <NuxtLink :href="material.url" :class="$style.info" target="_blank" rel="noopener noreferrer">
                <div :class="$style.favicon">
                    <img v-if="material.faviconUrl" :src="material.faviconUrl" alt="Favicon" />
                </div>

                <div :class="$style.fileInfo">
                    <p>{{ material.name }}</p>
                    <div :class="$style.fileDetails">
                        <p>{{ getHostname(material.url) }} • {{ new Date(material.createdAt).toLocaleDateString() }}</p>
                    </div>
                </div>
                <p :class="$style.description">{{ material.description }}</p>
            </NuxtLink>
            
            <div v-if="editMode" :class="$style.editButtons">
                <Button button-style="primary" accent-color="secondary" @click="emit('edit', material)" style="width: 100%">Upravit</Button>
                <Button button-style="secondary" accent-color="secondary" @click="emit('delete', material)" style="width: 100%">Smazat</Button>
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
        width: 100%;
        
        .fileIcon {
            mask-image: url('../../../public/icons/file.svg');
            mask-size: cover;
            mask-position: center;
            mask-repeat: no-repeat;

            width: 24px;
            height: 24px;
            background-color: var(--text-color-secondary);
            opacity: 0.6;
        }

        .fileInfo {
            display: flex;
            flex-direction: column;
            gap: 4px;
            border-right: 1px solid color-mix(in srgb, var(--text-color-secondary) 20%, transparent 40%);
            padding-right: 12px;

            p {
                margin: 0;
                font-size: 16px;
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
        }

        .description {
            margin-left: 10px;
            height: 100%;
            font-size: 14px;
            color: var(--text-color-secondary);
        }
    }
    
    .editButtons {
        display: flex;
        gap: 8px;
        padding: 12px 16px;
    }
}
</style>