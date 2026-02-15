<script setup lang="ts">
import Modal from "./Modal.vue";
import Button from "./Button.vue";

interface ModalInformativeProps {
    description: string;
    enabled: boolean;
    className?: string;
    okAction: () => void;
    okText?: string;
    title?: string | null;
    canBeClosedByClickingOutside?: boolean;
    descriptionTextAlign?: "left" | "center" | "right" | "justify";
}

const props = withDefaults(defineProps<ModalInformativeProps>(), {
    title: null,
    className: "",
    okText: "Rozumím",
    canBeClosedByClickingOutside: true,
    descriptionTextAlign: "center"
});

const emit = defineEmits<{
    (e: "close"): void;
}>();

const handleClose = () => {
    emit("close");
};

const handleOkClick = () => {
    props.okAction();
};
</script>

<template>
    <Modal
            :enabled="enabled"
            :can-be-closed-by-clicking-outside="canBeClosedByClickingOutside"
            :container-class-name="`${$style.modalinformative} ${className}`.trim()"
            :modal-class-name="$style['modal-content']"
            @close="handleClose"
    >
        <div :class="$style.closebutton" @click="handleClose"/>

        <div :class="$style.icon"/>

        <h1 v-if="title">{{ title }}</h1>

        <p :style="{ textAlign: descriptionTextAlign }">{{ description }}</p>

        <div :class="$style.buttons">
            <Button
                    button-style="primary"
                    @click="handleOkClick"
            >
                {{ okText }}
            </Button>
        </div>
    </Modal>
</template>

<style module lang="scss">
.modalinformative {

}
.modal-content {
    max-width: 330px;
    border-radius: 24px;
    padding: 24px;

    .closebutton {
        top: 12px;
        right: 12px;
    }

    >.icon {
        width: 64px;
        height: 64px;
        margin: 0 auto;
        mask-image: url(../../public/icons/warn.svg);
        mask-size: contain;
        mask-repeat: no-repeat;
        mask-position: center;
        position: relative;
        margin-bottom: 24px;

        &:before {
            content: '';
            position: absolute;
            width: 100%;
            height: 100%;
            background-color: var(--accent-color-primary);
            z-index: 1;
        }
    }

    >h1 {
        font-size: 24px;
        text-align: center;
        font-weight: 600;
        margin: 0;
        margin-bottom: 12px;
    }

    >p {
        //font-size: 20px;
        color: var(--text-color-secondary);

        >span {
            font-weight: 600;
            color: var(--accent-color);
        }
    }

    >.buttons {
        display: flex;
        gap: 16px;
        margin-top: 20px;
        width: 100%;
        position: relative;
        box-sizing: border-box;

        >button {
            flex-grow: 1;
            width: 100%;
        }
    }
}
</style>
