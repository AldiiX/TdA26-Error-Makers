<script setup lang="ts">
import type { ContextMenuItemType } from "./ContextMenu.vue"

const props = defineProps<{
    item: ContextMenuItemType
}>()

const emit = defineEmits<{
    (e: "click", item: ContextMenuItemType): void
}>()

function onClick() {
    if (props.item.disabled) return
    emit("click", props.item)
}
</script>

<template>
    <div
        :class="[$style.item, props.item.disabled && $style.disabled]"
        @click="onClick"
    >
        <p>
            <span 
                v-if="item.iconPath"
                :style="{
                    '--icon-path': `url(${item.iconPath})`
                }"
            />
            {{ item.text }}
        </p>
    </div>
</template>

<style module lang="scss">
.item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px;
    border-radius: 6px;
    cursor: pointer;
    user-select: none;
    transition: background-color 0.1s;

    &:hover {
        background: rgb(from var(--background-color) r g b / 0.5);
    }
    
    p {
        margin: 0;
        font-weight: 550;
        font-size: 16px;
        display: flex;
        align-items: center;
        gap: 8px;

        span {
            mask-image: var(--icon-path);
            mask-size: cover;
            width: 22px;
            height: 22px;
            display: inline-block;
            background-color: var(--text-color);
        }
    }

    &.disabled {
        opacity: 0.5;
        cursor: not-allowed;

        &:hover {
            background: transparent;
        }
    }

    .icon {
        width: 20px;
        height: 20px;
    }
}
</style>