<script setup lang="ts">
    const props = withDefaults(defineProps<{
        modelValue?: string | number | null,
        type?: string,
        placeholder?: string,
        required?: boolean,
    }>(), {
        modelValue: '',
        type: 'text',
        placeholder: '',
        required: false,
    });
    
    const emit = defineEmits<{
        (e: 'update:modelValue', value: string | number | null): void
    }>();
</script>

<template>
    <textarea
        v-if="type === 'textarea'"
        :placeholder="props.placeholder"
        :class="$style.input"
        :required="required"
        :value="modelValue"
        @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />
    <select
        v-else-if="type === 'select'"
        :class="$style.input"
        :required="required"
        :value="modelValue"
        @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    >
        <slot />
    </select>
    <input
        v-else
        :type="props.type"
        :placeholder="props.placeholder"
        :class="$style.input"
        :required="required"
        :value="modelValue"
        @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />
</template>

<style module lang="scss">
.input, .style {
    border-radius: 12px;
    padding: 12px 24px;
    font-size: 16px;
    background-color: var(--input-background-color);
    color: var(--text-color);
    font-family: Dosis, sans-serif;
    border: none;
    box-shadow: var(--input-shadow);
    outline: 2px solid transparent;

    &:focus {
        outline: 2px solid var(--accent-color);
    }
}
</style>