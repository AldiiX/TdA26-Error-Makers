<script setup lang="ts">
import Button from "~/components/Button.vue";

const props = defineProps<{
    isVisible: boolean;
    loading?: boolean;
}>();

const emit = defineEmits<{
    (e: "toggle"): void;
}>();
</script>

<template>
    <Button 
        :class="[$style.toggleButton, { [$style.visible]: isVisible }]"
        :loading="loading"
        @click="emit('toggle')"
        :title="isVisible ? 'Skrýt' : 'Zobrazit'"
        :buttonStyle="isVisible ? 'primary' : 'secondary'"
    >
            <div :class="$style.content">
                <span :class="[$style.icon, { [$style.iconHidden]: !isVisible }]" />
                <span :class="$style.label">{{ isVisible ? 'Viditelné' : 'Skryté' }}</span>
            </div>
    </Button>
</template>

<style module lang="scss">
.toggleButton {
    color: var(--accent-color-secondary-theme-text);
    min-width: 135px;
    
    .content {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 8px;
    }
    
    &.visible {
        border-color: color-mix(in srgb, var(--accent-color-primary) 40%, transparent 40%);
        background-color: color-mix(in srgb, var(--accent-color-primary) 10%, transparent 90%);
        //color: var(--accent-color-primary);
        
        &:hover {
            background-color: color-mix(in srgb, var(--accent-color-primary) 15%, transparent 85%);
        }
        
        .icon {
            background-color: var(--accent-color-secondary-theme-text);
        }
    }
    
    .icon {
        mask-image: url('/icons/eye.svg');
        mask-size: cover;
        mask-position: center;
        mask-repeat: no-repeat;
        display: inline-block;
        
        width: 20px;
        height: 20px;
        min-width: 20px;
        background-color: var(--accent-color-primary);
        transition: background-color 0.3s;
        
        &.iconHidden {
            mask-image: url('/icons/eye-slash.svg');
        }
    }
    
    .label {
        font-weight: 500;
    }
}
</style>



