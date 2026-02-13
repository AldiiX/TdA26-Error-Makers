<script setup lang="ts">
import {type DbStatus, statusToText} from "#shared/utils/statusMapper";

const props = defineProps<{
    status: DbStatus,
}>();
</script>

<template>
    <div :class="[$style.statusIcon, props.status === 1 ? $style.scheduled : '', props.status === 2 ? $style.live : '', props.status === 3 ? $style.paused : '']">
        <div></div>
        <p>{{ statusToText(props.status) }}</p>
    </div>
</template>

<style module lang="scss">
.statusIcon {
    pointer-events: none;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 500px;
    opacity: .8;
    height: 32px;
    padding-left: 4px;
    padding-right: 8px;
    
    p {
        font-weight: 600;
    }
    
    div {
        mask-size: cover;
        width: 30px;
        height: 30px;
        mask-position: center;
    }

    &.scheduled {
        background-color: var(--status-scheduled-bg);
        //padding: 4px;

        div {
            mask-image: url("../../public/icons/clock.svg");
            background-color: var(--status-scheduled-text);

            width: 24px;
            height: 24px;
            margin-right: 4px;
        }
        
        p {
            color: var(--status-scheduled-text);
        }
    }

    &.live {
        background-color: var(--status-live-bg);
        //padding: 2px;

        div {
            mask-image: url("../../public/icons/access_point.svg");
            background-color: var(--status-live-text);

            margin-right: 2px;
        }
        
        p {
            color: var(--status-live-text);
        }
    }

    &.paused {
        background-color: var(--status-paused-bg);
        //padding: 4px;

        div {
            mask-image: url("../../public/icons/pause.svg");
            background-color: var(--status-paused-text);

            width: 24px;
            height: 24px;
            margin-right: 2px;
        }
        
        p {
            color: var(--status-paused-text);
        }
    }
}
</style>