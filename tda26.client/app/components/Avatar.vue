<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import type { CSSProperties } from "vue";
import getHslFromText from "#shared/utils/getHslFromText";

const AVATAR_ENABLED: boolean = true;

type Size = string | number;
type BackgroundColor = string | "random" | null;

type AvatarProps = {
    size?: Size;
    name: string;
    src: string | null;
    className?: string;
    backgroundColor?: BackgroundColor;
    containerStyle?: CSSProperties;
    letterStyle?: CSSProperties;
};

const props = withDefaults(defineProps<AvatarProps>(), {
    size: 16,
    src: null,
    className: "",
    backgroundColor: null
});

const isClient = ref(false);
onMounted(() => { isClient.value = true; });

const sizeComputed = computed<string>(() => {
    return typeof props.size === "number" ? `${props.size}px` : props.size;
});

const computedBg = computed<string>(() => {
    if (props.backgroundColor === "random") { return getHslFromText(props.name); }
    if (props.backgroundColor === null) { return "var(--accent-color-secondary-theme)"; }
    return props.backgroundColor;
});

type StyleVars = CSSProperties & { ["--size"]?: string };
const containerStyles = computed<StyleVars>(() => {
    return {
        backgroundColor: (!props.src || !AVATAR_ENABLED) ? computedBg.value : "transparent",
        "--size": sizeComputed.value,
        ...(props.containerStyle ?? {})
    };
});

const letter = computed<string>(() => {
    const l = props.name?.split(" ").map(w => w?.[0] ?? "").join("").slice(0, 2);
    return (l || "").toUpperCase();
});

const emit = defineEmits<{ (e: "click"): void }>();
</script>

<template>
    <div
            :class="[$style.avatar, className]"
            :style="containerStyles"
            @click="emit('click')"
    >
        <img v-if="src && AVATAR_ENABLED" :src="src" alt="avatar" />
        <p v-else-if="isClient" :style="letterStyle">{{ letter }}</p>
    </div>
</template>

<style lang="scss" module>
.avatar {
    width: var(--size);
    aspect-ratio: 1/1;
    border-radius: 100%;
    position: relative;
    flex-shrink: 0;
    overflow: hidden;

    p {
        margin: 0;
    }

    >p {
        font-size: calc(var(--size) / 2.3);
        width: fit-content;
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        user-select: none;
        pointer-events: none;
        color: var(--bg);
        font-weight: 450;
    }

    img {
        position: absolute;
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 100%;
        pointer-events: none;
        //border: 1px solid var(--text-color-darker);
    }
}
</style>