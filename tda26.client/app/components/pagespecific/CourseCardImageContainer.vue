<script setup lang="ts">
    import { NuxtLink } from "#components";
    import type {Course} from "#shared/types";
    import {ref, type Ref, shallowRef, toRef} from "vue";

    const props = defineProps<{
        course: Course;
        class?: string;
    }>();

    const courseRef = toRef(props, 'course') as Ref<Course>;
    const displayedImageUrl = computed(() => courseRef.value.imageUrl);
</script>

<template>
    <NuxtLink :to="`/courses/${course.uuid}`" :class="[$style.imageContainer, props.class]">
        <div :class="$style.image" v-if="displayedImageUrl" :style="{ '--bg': `url(${displayedImageUrl})` }"></div>

        <template v-if="!displayedImageUrl">
            <div :class="$style.blob1"></div>
            <div :class="$style.blob2"></div>
            <div :class="$style.blob3"></div>
            <div :class="$style.blob4"></div>
            <div :class="$style.blob5"></div>

            <div :class="$style.circle">
                <div :class="$style.icon" :style="{ maskImage: `url(${courseRef.imageUrlOrDefault})` }"></div>
            </div>
        </template>
    </NuxtLink>
</template>

<style module lang="scss">

.imageContainer {
    display: block;
    min-height: 200px;
    width: 100%;
    background: linear-gradient(160deg, var(--accent-color-primary), var(--accent-color-primary-darker));
    overflow: hidden;
    border-radius: 24px;
    transition: filter 0.3s;
    position: relative;

    >* {
        pointer-events: none;
    }

    &:hover {
        filter: brightness(0.75);
        transition-duration: 0.3s;
    }

    .image {
        width: 100%;
        height: 100%;
        background-image: var(--bg);
        background-size: cover;
        background-position: center;
        position: absolute;
    }

    .blob1 {
        width: 32px;
        aspect-ratio: 1/1;
        position: absolute;
        top: 16%;
        left: 10%;
        background: white;
        opacity: 0.1;
        mask: linear-gradient(to bottom right, black, transparent);
        border-radius: 50%;
    }

    .blob2 {
        width: 64px;
        aspect-ratio: 1/1;
        position: absolute;
        bottom: 8%;
        left: 6%;
        background: black;
        opacity: 0.1;
        mask: linear-gradient(to bottom right, black, transparent);
        border-radius: 50%;
    }

    .blob3 {
        width: 24px;
        aspect-ratio: 1/1;
        position: absolute;
        top: 12%;
        right: 6%;
        background: black;
        opacity: 0.1;
        mask: linear-gradient(to bottom right, black, transparent);
        border-radius: 50%;
    }

    .blob4 {
        width: 92px;
        aspect-ratio: 1/1;
        position: absolute;
        bottom: 12%;
        right: -6%;
        background: white;
        opacity: 0.1;
        mask: linear-gradient(206deg, black, transparent);
        border-radius: 50%;
    }

    .blob5 {
        width: 48px;
        aspect-ratio: 1/1;
        position: absolute;
        bottom: 2%;
        right: 12%;
        background: black;
        opacity: 0.1;
        mask: linear-gradient(36deg, black, transparent);
        border-radius: 50%;
    }

    .circle {
        position: absolute;
        width: calc(64px + 40px);
        aspect-ratio: 1/1;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        border-radius: 50%;
        background: linear-gradient(135deg, var(--accent-color-secondary-transparent-03), var(--accent-color-secondary-transparent-01));
        box-shadow: 0 0 32px rgba(0, 0, 0, 0.1);

        .icon {
            position: absolute;
            width: 50%;
            height: 50%;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background-color: var(--accent-color-secondary-transparent-06);
            mask-size: contain;
            mask-position: center;
            mask-repeat: no-repeat;
            //mask-image: url(/icons/courseicons/paint.svg);
        }
    }
}
</style>