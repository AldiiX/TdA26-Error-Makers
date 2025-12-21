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
    <p
        class="tag"
        :data-active="active"
        @click="toggle"
    >
        {{ tag.displayName }}
    </p>
</template>

<style scoped>
.tag {
    background-color: var(--background-color-3);
    padding: 8px 16px;
    border-radius: 999px;
    margin: 0;
    font-size: 14px;
    cursor: pointer;
    user-select: none;
    transition-duration: 0.3s;
}

.tag:hover {
    background: var(--background-color-primary);
}

.tag[data-active="true"] {
    background: var(--accent-color-primary);
    color: var(--accent-color-primary-text);
}
</style>