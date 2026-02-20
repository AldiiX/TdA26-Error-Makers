<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

const props = defineProps<{
    position?: 'bottom-right' | 'bottom-left' | 'top-right' | 'top-left';
    trigger?: 'click' | 'hover';
    wrapperClass?: string;
    teleport?: boolean;
    disabled?: boolean;
}>();

const isOpen = ref(false);
const popoverRef = ref<HTMLDivElement | null>(null);
const triggerRef = ref<HTMLDivElement | null>(null);
const hoverTimeout = ref<number | null>(null);

// Teleport positioning
const teleportPosition = ref({ top: 0, left: 0 });

const updateTeleportPosition = (event: MouseEvent) => {
    const triggerRect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    teleportPosition.value = {
        top: triggerRect.top + window.scrollY - (popoverRef.value?.offsetHeight || 0) - 8,
        left: triggerRect.left + window.scrollX + triggerRect.width / 2,
    };
};

// Regular popover
const openPopover = () => {
    if (hoverTimeout.value) {
        clearTimeout(hoverTimeout.value);
        hoverTimeout.value = null;
    }
    isOpen.value = true;
};

const closePopover = () => {
    isOpen.value = false;
};

const closePopoverDelayed = () => {
    hoverTimeout.value = window.setTimeout(() => {
        closePopover();
    }, 200);
};

const togglePopover = () => {
    isOpen.value = !isOpen.value;
};

const handleClickOutside = (event: MouseEvent) => {
    if (props.trigger === 'click' && popoverRef.value && triggerRef.value) {
        const target = event.target as Node;
        if (!popoverRef.value.contains(target) && !triggerRef.value.contains(target)) {
            closePopover();
        }
    }
};

const handleMouseEnter = async (event: MouseEvent) => {
    if (props.disabled) return;
    if (props.teleport) {
        isOpen.value = true;
        await nextTick();
        updateTeleportPosition(event);
    } else if (props.trigger === 'hover') {
        openPopover();
    }
};

const handleMouseLeave = () => {
    if (props.teleport) {
        isOpen.value = false;
    } else if (props.trigger === 'hover') {
        closePopoverDelayed();
    }
};

const handleClick = () => {
    if (!props.teleport && (props.trigger === 'click' || !props.trigger)) {
        togglePopover();
    }
};

onMounted(() => {
    if (!props.teleport && (props.trigger === 'click' || !props.trigger)) {
        document.addEventListener('click', handleClickOutside);
    }
});

onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside);
    if (hoverTimeout.value) {
        clearTimeout(hoverTimeout.value);
    }
});

const $style = useCssModule();
</script>

<template>
    <!-- Teleport mode -->
    <div
        v-if="teleport"
        :class="[$style.popoverWrapper, props.wrapperClass]"
        @mouseenter="handleMouseEnter"
        @mouseleave="handleMouseLeave"
    >
        <slot name="trigger"/>

        <Teleport to="#teleports">
            <Transition name="popover-fade-teleport">
                <div
                    v-if="!disabled && isOpen"
                    ref="popoverRef"
                    :class="[$style.teleportContent, 'liquid-glass']"
                    :style="{
                        top: teleportPosition.top + 'px',
                        left: teleportPosition.left + 'px',
                    }"
                >
                    <slot name="content"/>
                </div>
            </Transition>
        </Teleport>
    </div>

    <!-- Regular mode -->
    <div v-else :class="[$style.popoverWrapper, props.wrapperClass]">
        <div
            ref="triggerRef"
            :class="$style.trigger"
            @click="handleClick"
            @mouseenter="handleMouseEnter"
            @mouseleave="handleMouseLeave"
        >
            <slot name="trigger"/>
        </div>

        <Transition name="popover-fade">
            <div
                v-if="isOpen"
                ref="popoverRef"
                :class="[$style.popover, $style[position || 'bottom-right']]"
                @mouseenter="handleMouseEnter"
                @mouseleave="handleMouseLeave"
            >
                <slot name="content"/>
            </div>
        </Transition>
    </div>
</template>

<style module lang="scss">
.popoverWrapper {
    position: relative;
    display: inline-block;
}

.trigger {
    cursor: pointer;
    display: flex;
    align-items: center;
}

.popover {
    position: absolute;
    background-color: var(--background-color-secondary);
    border-radius: 16px;
    padding: 16px;
    min-width: 256px;
    z-index: 10;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 0.6);
    backdrop-filter: blur(8px);

    &.bottom-right {
        top: calc(100% + 16px);
        right: 0;
    }

    &.bottom-left {
        top: calc(100% + 10px);
        left: 0;
    }

    &.top-right {
        bottom: calc(100% + 10px);
        right: 0;
    }

    &.top-left {
        bottom: calc(100% + 10px);
        left: 0;
    }
}

.teleportContent {
    position: absolute;
    bottom: 100%;
    left: 50%;
    transform: translateX(-50%);
    padding: 8px 12px;
    border-radius: 6px;
    z-index: 1000;
    width: max-content;
    height: max-content;
    max-width: min(350px, 95vw);
    text-align: center;
    pointer-events: none;
}
</style>

<style lang="scss">
.popover-fade-enter-active,
.popover-fade-leave-active,
.popover-fade-teleport-enter-active,
.popover-fade-teleport-leave-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
}

.popover-fade-enter-from {
    opacity: 0;
    transform: translateY(-10px) scale(0.95);
}

.popover-fade-leave-to {
    opacity: 0;
    transform: translateY(-10px) scale(0.95);
}

.popover-fade-teleport-enter-from {
    opacity: 0;
}

.popover-fade-teleport-leave-to {
    opacity: 0;
}
</style>
