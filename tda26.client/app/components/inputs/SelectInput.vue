<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps({
    modelValue: { type: String, default: '' },
    label: { type: String, default: '' },
    options: { type: Array as () => { value: string; label: string }[], default: () => [] },
    required: { type: Boolean, default: false },
    id: { type: String, default: undefined as string | undefined }
});
const emit = defineEmits(['update:modelValue']);

const value = computed({
    get: () => props.modelValue,
    set: (v: string) => emit('update:modelValue', v)
});
</script>

<template>
    <label :for="id" class="block">
        <div class="text-sm font-medium mb-1">{{ label }} <span v-if="required">*</span></div>
        <select :id="id" class="w-full p-2 border rounded" v-model="value" :aria-required="required">
            <option value="" disabled>Select…</option>
            <option v-for="opt in options" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
        </select>
    </label>
</template>
