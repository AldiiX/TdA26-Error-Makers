<script setup lang="ts">
const props = defineProps<{
    tag: {
        uuid: string
        displayName: string
    }
    active: boolean
}>()

const emit = defineEmits(["toggle"])

const toggle = () => emit("toggle", props.tag.uuid)
</script>

<template>
    <button
        class="tag"
        :data-active="active"
        @click="toggle"
    >
        {{ tag.displayName }}
    </button>
</template>

<style scoped>
.tag {
    display: flex;
    width: auto;
    height: auto;
    padding: 8px 12px;
    font-size: 16px;
    justify-content: center;
    align-items: center;
    border-radius: 16px;
    border: 2px solid var(--accent-color-secondary-theme);
}

.tag:hover {
    background: var(--accent-color-secondary-theme);
    color: var(--accent-color-secondary-theme-text);
}

.tag[data-active="true"] {
    background: var(--accent-color-secondary);
    color: var(--accent-color-secondary-theme-text);
    border-color: var(--accent-color-secondary);
}
</style>