<script setup lang="ts">
import { statusToText } from "#shared/utils/statusMapper";
import type {CourseStatus} from "#shared/types";

const props = defineProps<{
    status: CourseStatus,
}>();
</script>

<template>
    <div :class="[$style.statusIcon, props.status === 'draft' ? $style.draft : props.status === 'scheduled' ? $style.scheduled : props.status === 'live' ? $style.live : props.status === 'paused' ? $style.paused : $style.archived]">
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
    
    &.draft {
        background-color: var(--status-draft-bg);
        //padding: 4px;

        div {
            mask-image: url("../../public/icons/file-full.svg");
            background-color: var(--status-draft-text);

            width: 18px;
            height: 18px;
            margin-bottom: 1px;

        }

        p {
            color: var(--status-draft-text);
        }
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

    &.archived {
        background-color: var(--status-archived-bg);
        //padding: 4px;

        div {
            mask-image: url("../../public/icons/box-archive.svg");
            background-color: var(--status-archived-text);

        }

        p {
            color: var(--status-archived-text);
        }
    }
}
</style>