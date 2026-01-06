<script setup lang="ts">
import Modal from "./Modal.vue";
import Button from "./Button.vue";

interface ModalDestructiveProps {
    description: string;
    enabled: boolean;
    className?: string;
    yesAction: () => void;
    yesText?: string;
    noAction?: () => void;
    noText?: string;
    title?: string;
    canBeClosedByClickingOutside?: boolean;
}

const props = withDefaults(defineProps<ModalDestructiveProps>(), {
    title: "Potvrzení akce",
    className: "",
    yesText: "Ano",
    noText: "Ne",
    canBeClosedByClickingOutside: false
});

const emit = defineEmits<{
    (e: "close"): void;
}>();

const handleClose = () => {
    // zavreni destruktivniho modalu
    emit("close");
};

const handleYesClick = () => {
    // potvrzeni destruktivni akce
    props.yesAction();
};

const handleNoClick = () => {
    // zruseni akce
    if (props.noAction) {
        props.noAction();
    } else {
        handleClose();
    }
};
</script>

<template>
    <Modal
            :enabled="enabled"
            :can-be-closed-by-clicking-outside="canBeClosedByClickingOutside"
            :container-class-name="`${$style.modaldestructive} ${className}`.trim()"
            :modal-class-name="$style['modal-content']"
            @close="handleClose"
    >
        <div :class="$style.closebutton" @click="handleClose"></div>

        <div :class="$style.icon"></div>

        <h1>{{ title }}</h1>

        <p>{{ description }}</p>

        <div :class="$style.buttons">
            <Button
                    button-style="secondary"
                    @click="handleNoClick"
            >
                {{ noText }}
            </Button>

            <Button
                    button-style="primary"
                    accent-color="primary"
                    @click="handleYesClick"
            >
                {{ yesText }}
            </Button>
        </div>
    </Modal>
</template>

<style module lang="scss">
.modaldestructive {

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
        text-align: center;
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
            width: calc(100% / 2 - 8px);
            padding: 12px;
        }
    }
}
</style>