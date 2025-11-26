<script setup lang="ts">
import { reactive, toRefs, watch } from 'vue';
import TextInput from '~/components/inputs/TextInput.vue';
import TextArea from '~/components/inputs/TextArea.vue';
import SelectInput from '~/components/inputs/SelectInput.vue';

type MaterialLocal = {
    name: string;
    description?: string | null;
    type: string;
    url?: string | null;
};

const props = defineProps({
    modelValue: {
        type: Object as () => MaterialLocal,
        required: true
    },
    index: { type: Number, required: false }
});
const emit = defineEmits(['update:modelValue', 'remove']);

const local = reactive<MaterialLocal>({
    name: props.modelValue.name ?? '',
    description: props.modelValue.description ?? '',
    type: props.modelValue.type ?? '',
    url: props.modelValue.url ?? ''
});

watch(
    () => props.modelValue,
    (nv) => {
        local.name = nv.name ?? '';
        local.description = nv.description ?? '';
        local.type = nv.type ?? '';
        local.url = nv.url ?? '';
    },
    { deep: true }
);

watch(
    local,
    (n) => emit('update:modelValue', { ...n }),
    { deep: true }
);

const typeOptions = [
    { value: 'file', label: 'File (upload later)' },
    { value: 'url', label: 'URL' },
    { value: 'other', label: 'Other' }
];

const remove = () => emit('remove');
</script>

<template>
    <div class="p-3 border rounded mb-3">
        <div class="flex justify-between items-start gap-2">
            <h4 class="font-medium">Material {{ (index ?? 0) + 1 }}</h4>
            <button type="button" class="text-sm text-red-600" @click="remove">Remove</button>
        </div>

        <div class="grid grid-cols-1 gap-2 mt-2">
            <TextInput v-model="local.name" label="Name" required :maxlength="128" />
            <TextArea :v-model="local.description" label="Description" :maxlength="1048" />
            <SelectInput v-model="local.type" :options="typeOptions" label="Type" required />
            <TextInput
                v-if="local.type === 'url'"
                :v-model="local.url"
                label="URL"
                required
                :maxlength="256"
                placeholder="https://example.com/..."
            />
        </div>
    </div>
</template>
