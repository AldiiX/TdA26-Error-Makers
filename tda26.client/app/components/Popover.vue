<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

const props = defineProps<{
    position?: 'bottom-right' | 'bottom-left' | 'top-right' | 'top-left'
}>();

const isOpen = ref(false);
const popoverRef = ref<HTMLDivElement | null>(null);
const triggerRef = ref<HTMLDivElement | null>(null);

const togglePopover = () => {
    isOpen.value = !isOpen.value;
};

const closePopover = () => {
    isOpen.value = false;
};

const handleClickOutside = (event: MouseEvent) => {
    if (popoverRef.value && triggerRef.value) {
        const target = event.target as Node;
        if (!popoverRef.value.contains(target) && !triggerRef.value.contains(target)) {
            closePopover();
        }
    }
};

onMounted(() => {
    document.addEventListener('click', handleClickOutside);
});

onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside);
});

const $style = useCssModule();
</script>

<template>
    <div :class="$style.popoverWrapper">
        <div 
            ref="triggerRef" 
            :class="$style.trigger"
            @click="togglePopover"
        >
            <slot name="trigger"></slot>
        </div>
        
        <Transition name="popover-fade">
            <div 
                v-if="isOpen"
                ref="popoverRef"
                :class="[$style.popover, $style[position || 'bottom-right']]"
            >
                <slot name="content"></slot>
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
    z-index: 1000;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 0.6);
    backdrop-filter: blur(8px);
    
    &.bottom-right {
        top: calc(100% + 10px);
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
</style>

<style scoped>
.popover-fade-enter-active,
.popover-fade-leave-active {
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
</style>
