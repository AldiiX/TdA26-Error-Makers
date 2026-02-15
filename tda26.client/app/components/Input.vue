<script setup lang="ts">
    import { push } from "notivue";

    const props = withDefaults(defineProps<{
        modelValue?: string | number | null,
        type?: string,
        placeholder?: string,
        required?: boolean,
        defaultFileName?: string,
        allowedFileTypes?: string[] | null,
        maxFileSize?: number | null,
    }>(), {
        modelValue: '',
        type: 'text',
        placeholder: '',
        required: false,
        defaultFileName: 'Nahrát soubor',
        allowedFileTypes: null,
        maxFileSize: 30 * 1024 * 1024, // 30 MB
    });
    
    const emit = defineEmits<{
        (e: 'update:modelValue', value: string | File | undefined | null): void
    }>();

    const fileName = ref(props.defaultFileName);

    function onFileChange(e: Event) {
        const target = e.target as HTMLInputElement;
        const selected = target.files?.[0] ?? null;

        if (!selected) {
            fileName.value = props.defaultFileName;
            emit('update:modelValue', null);
            return;
        }

        // max size check
        if (props.maxFileSize && selected.size > props.maxFileSize) {
            target.value = '';
            fileName.value = props.defaultFileName;
            emit('update:modelValue', null);
            push.error({
                title: "Chyba nahrávání souboru",
                message: `Soubor je příliš velký! Limit: ${Math.round(props.maxFileSize / 1024 / 1024)} MB`,
                duration: 5000
            });
            return;
        }
        
        // mime type check
        if (props.allowedFileTypes && !props.allowedFileTypes.includes(selected.type)) {
            target.value = '';
            fileName.value = props.defaultFileName;
            emit('update:modelValue', null);
            push.error({
                title: "Chyba nahrávání souboru",
                message: "Nepodporovaný formát souboru!",
                duration: 5000
            });
            return;
        }

        fileName.value = selected.name;
        emit('update:modelValue', selected);
    }
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
        @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
    >
        <slot />
    </select>
    <label v-else-if="type === 'file'" :class="[$style.input, $style.fileInput]">
        <input
            :type="props.type"
            :placeholder="props.placeholder"
            :required="required"
            :accept="props.allowedFileTypes?.join(',')"
            @change="onFileChange"
        >
        <i>{{ fileName }}</i>
    </label>
    <input
        v-else
        :type="props.type"
        :placeholder="props.placeholder"
        :class="$style.input"
        :required="required"
        :value="modelValue"
        @input="emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    >
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
    user-select: none;

    &:focus {
        outline: 2px solid var(--accent-color);
    }
    
    [type="file"] {
        display: none;
    }
}

.fileInput {
    border: 2px dashed var(--input-border-color);
    
    i {
        display: flex;
        align-items: center;
        
        &::before {
            content: '';
            display: inline-block;
            width: 24px;
            height: 24px;
            margin-right: 8px;
            mask-image: url('../../public/icons/file.svg');
            mask-size: cover;
            background-color: var(--text-color);
            opacity: .7;
        }
    }
}

</style>