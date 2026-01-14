<script setup lang="ts">
import { ref, watch, onBeforeUnmount } from "vue";
import type { CSSProperties } from "vue";

type AnimationState = "opening" | "open" | "closing" | "closed";

interface ModalProps {
    enabled: boolean;
    canBeClosedByClickingOutside?: boolean;
    modalStyle?: CSSProperties;
    modalClassName?: string;
    containerStyle?: CSSProperties;
    containerClassName?: string;
    showCloseButton?: boolean;
    closeButtonClassName?: string;
    className?: string;
}

const props = withDefaults(defineProps<ModalProps>(), {
    canBeClosedByClickingOutside: true,
    modalClassName: "",
    containerClassName: "",
    showCloseButton: true,
    closeButtonClassName: "",
    className: ""
});

const emit = defineEmits<{
    (e: "close"): void;
}>();

const isOpen = ref(props.enabled);
const animationState = ref<AnimationState>("closed");
let animationTimeout: ReturnType<typeof setTimeout> | null = null;

const clearAnimationTimeout = () => {
    if (animationTimeout !== null) {
        clearTimeout(animationTimeout);
        animationTimeout = null;
    }
};

watch(
    () => props.enabled,
    (enabled) => {
        clearAnimationTimeout();

        if (enabled) {
            isOpen.value = true;
            animationState.value = "opening";

            animationTimeout = setTimeout(() => {
                animationState.value = "open";
            }, 300);
        } else {
            animationState.value = "closing";

            animationTimeout = setTimeout(() => {
                isOpen.value = false;
                animationState.value = "closed";
            }, 250);
        }
    },
    { immediate: true }
);

const handleClose = () => {
    clearAnimationTimeout();
    emit("close");
};

const handleKeydown = (event: KeyboardEvent) => {
    if (event.key === "Escape") handleClose();
};

watch(
    () => isOpen.value,
    (open) => {
        if (typeof document === "undefined") return; // <-- SSR SAFE

        if (open) {
            document.addEventListener("keydown", handleKeydown);
        } else {
            document.removeEventListener("keydown", handleKeydown);
        }
    },
    { immediate: true }
);

onBeforeUnmount(() => {
    if (typeof document === "undefined") return; // <-- SSR SAFE
    document.removeEventListener("keydown", handleKeydown);
    clearAnimationTimeout();
});
</script>

<template>
    <div
        v-if="isOpen"
        :class="[$style.modal, props.containerClassName, props.className]"
        :style="props.containerStyle"
        :data-anim="animationState"
    >
        <div
            :class="$style.blurdiv"
            @click="props.canBeClosedByClickingOutside ? handleClose() : undefined"
        ></div>

        <div
            :class="[$style.modalcontent, props.modalClassName]"
            :style="props.modalStyle"
        >
            <div
                v-if="props.showCloseButton"
                :class="[$style.closebutton, props.closeButtonClassName]"
                @click="handleClose"
            ></div>

            <slot />
        </div>
    </div>
</template>

<style module lang="scss">
.modal {
    position: fixed;
    z-index: 15000;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;

    &[data-anim="opening"] {
        .blurdiv {
            animation: closediv-opening 0.3s ease-out forwards;
            @keyframes closediv-opening {
                0% { opacity: 0; }
                100% { opacity: 1; }
            }
        }
        .modalcontent {
            animation: modalcontent-opening 0.3s ease-in-out forwards;
            @keyframes modalcontent-opening {
                0% {
                    transform: translate(-50%, -65%);
                    opacity: 0;
                }
                100% {
                    transform: translate(-50%, -50%);
                    opacity: 1;
                }
            }
        }
    }

    &[data-anim="closing"] {
        .blurdiv {
            animation: closediv-closing 0.25s ease-out forwards;
            @keyframes closediv-closing {
                0% { opacity: 1; }
                100% { opacity: 0; }
            }
        }
        .modalcontent {
            animation: modalcontent-closing 0.15s ease-in forwards;
            @keyframes modalcontent-closing {
                0% {
                    transform: translate(-50%, -50%);
                    opacity: 1;
                }
                100% {
                    transform: translate(-50%, -80%);
                    opacity: 0;
                }
            }
        }
    }

    > .blurdiv {
        position: fixed;
        left: 0; top: 0;
        width: 100%; height: 100%;
        opacity: 1;
        backdrop-filter: blur(5px) brightness(0.6);
    }

    .modalcontent {
        position: absolute;
        left: 50%; top: 50%;
        transform: translate(-50%, -50%);
        background-color: var(--background-color-primary);
        padding: 32px;
        border: 1px solid var(--background-color-secondary);
        border-radius: 24px;
        width: 90vw;
        max-width: 400px;
        min-width: 40px;
        max-height: 90vh;
        min-height: 40px;
        box-shadow: 0 0 6px rgba(0, 0, 0, 0.025);
        overflow: auto;
        scrollbar-width: none;
        -ms-overflow-style: none;

        &::-webkit-scrollbar {
            display: none;
        }

        > .closebutton {
            $size: 16px;

            width: 32px;
            aspect-ratio: 1/1;
            position: absolute;
            background-color: var(--modal-input-background-color);
            z-index: 1;
            border-radius: 100%;
            top: 12px;
            right: 12px;
            transition-duration: 0.3s;
            cursor: pointer;

            &:hover {
                transition-duration: 0.3s;
                background-color: var(--accent-color-primary-transparent-01);
            }

            &::after {
                position: absolute;
                content: "";
                width: 100%;
                height: 100%;
                top: 0;
                left: 0;
                mask-image: url("../../public/icons/x.svg");
                mask-size: $size;
                mask-repeat: no-repeat;
                mask-position: center;
                background-color: var(--text-color-secondary);
            }
        }
    }
}

@media (prefers-reduced-motion: reduce) {
    .modal, .modal * {
        animation-duration: 0s !important;
        transition-duration: 0s !important;
    }
}
</style>
