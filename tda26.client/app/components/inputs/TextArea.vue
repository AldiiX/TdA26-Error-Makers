<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps({
    modelValue: { type: String, default: '' },
    label: { type: String, default: '' },
    placeholder: { type: String, default: '' },
    required: { type: Boolean, default: false },
    maxlength: { type: Number, default: undefined as number | undefined },
    rows: { type: Number, default: 4 },
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
        <textarea
            :id="id"
            class="w-full p-2 border rounded"
            :placeholder="placeholder"
            :maxlength="maxlength"
            :rows="rows"
            v-model="value"
        />
    </label>
</template>
