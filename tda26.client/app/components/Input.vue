<script setup lang="ts">
    // propy deděné z rodiče
    const props = defineProps(
        {
            placeholder: {
                type: String,
                required: false,
                default: ""
            },

            type: {
                type: String,
                required: false,
                default: "text"
            }
        }
    )
</script>

<template>
    <input
            :type="props.type"
            :placeholder="props.placeholder"
            :class="$style.input"
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