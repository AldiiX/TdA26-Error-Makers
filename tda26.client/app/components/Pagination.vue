<script setup lang="ts">
import { computed } from "vue";

const props = defineProps<{
    page: number;
    totalPages: number;
    visiblePages: (number | "...")[];
}>();

const emits = defineEmits<{
    (e: "update:page", value: number): void;
}>();

const goToPage = (p: number) => {
    if (p < 1 || p > props.totalPages) return;
    emits("update:page", p);
};

const goPrev = () => goToPage(props.page - 1);
const goNext = () => goToPage(props.page + 1);
</script>

<template>
    <div :class="$style.paginationContainer">

        <div :class="[$style.arrow, $style.leftArrow]"
             @click="goPrev"></div>

        <template v-for="(p, idx) in props.visiblePages" :key="idx">

            <div
                v-if="p !== '...'"
                :class="[ $style.pageNumber, props.page === p && $style.active ]"
                @click="goToPage(Number(p))"
            >
                {{ p }}
            </div>

            <div v-else :class="$style.dots">...</div>

        </template>

        <div :class="[$style.arrow, $style.rightArrow]"
             @click="goNext"></div>

    </div>
</template>

<style module lang="scss">
.paginationContainer {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 12px;

    background: var(--background-color-secondary);
    padding: 12px 24px;
    border-radius: 32px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);

    .arrow {
        width: 36px;
        height: 36px;
        border-radius: 100%;
        cursor: pointer;

        background-image: url("../../public/icons/arrow.svg");
        background-repeat: no-repeat;
        background-position: center;
        background-size: 18px;

        transition: 0.2s ease;

        &:hover {
            background-color: var(--accent-color-primary);
            opacity: 0.9;
        }
    }

    .leftArrow {
        transform: rotate(180deg);
    }

    .rightArrow {
        transform: rotate(0deg);
    }

    .pageNumber {
        font-size: 16px;
        cursor: pointer;
        width: 36px;
        height: 36px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-family: "Dosis", sans-serif;

        border-radius: 100%;
        color: var(--text-color-primary);
        transition: 0.2s ease;

        &:hover {
            opacity: 0.8;
            background: var(--accent-color-secondary-theme);
            color: var(--accent-color-primary-text);
        }

        &.active {
            background: var(--accent-color-primary);
            color: var(--accent-color-primary-text);
        }
    }

    .dots {
        width: 36px;
        height: 36px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-family: "Dosis" ,sans-serif;
        font-weight: 900;
        font-size: 16px;
        opacity: 0.5;
        user-select: none;
    }
}
</style>
