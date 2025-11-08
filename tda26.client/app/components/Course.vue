<script setup lang="ts">
    import type { Course } from "~/lib/types";
    
    defineProps<{
        course: Course
    }>();
</script>

<template>
    <div :class="$style.container">
        <p> {{ course.name }}</p>
    </div>
</template>

<style module lang="scss">
.container {
    display: flex;
    width: 256px;
    height: 10px;
    background-color: var(--accent-color-primary);
}
</style>
