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
    padding: 4px 8px;
    backdrop-filter: blur(10px);

    &:is(.live) {
        >div {
            animation: pulse 2s ease infinite;

            @keyframes pulse {
                0% {
                    opacity: 1;
                }
                50% {
                    opacity: .5;
                }
                100% {
                    opacity: 1;
                }
            }
        }
    }
    
    >p {
        font-weight: 600;
        margin: 0;
        margin-bottom: 1px;
    }
    
    >div {
        mask-size: cover;
        width: 16px;
        height: 16px;
        mask-position: center;
        margin-right: 4px;
    }

    &.scheduled {
        background-color: var(--status-scheduled-bg);
        //padding: 4px;

        div {
            mask-image: url("../../public/icons/clock.svg");
            background-color: var(--status-scheduled-text);

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
        }
        
        p {
            color: var(--status-paused-text);
        }
    }
}
</style>