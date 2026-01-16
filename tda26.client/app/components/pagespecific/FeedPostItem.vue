<script setup lang="ts">
import type { FeedPostView } from "#shared/types";
import Button from "~/components/Button.vue";
import Avatar from "~/components/Avatar.vue";
import { defineEmits, computed } from "vue";
import timeAgoString from "#shared/utils/timeAgoString";

const props = defineProps<{
    feedPost: FeedPostView,
    editMode?: boolean
}>();

const emit = defineEmits<{
    (e: "edit", post: FeedPostView): void;
    (e: "delete", post: FeedPostView): void;
}>();

const feedPostStyle = computed(() => ({
    borderLeft: `8px solid var(${props.feedPost.background})`
}));
</script>

<template>
    <div :class="$style.feedPostWrapper">
        <div :class="$style.feedPostLeft" :style="feedPostStyle">
            <div :class="$style.iconWrapper" :style="{ backgroundColor: `var(${props.feedPost.background})` }">
                <div
                    :class="$style.feedPostIcon"
                    :style="{ maskImage: `url(${props.feedPost.icon})` }"
                />
            </div>
        </div>

        <div :class="$style.feedPostRight">
            <div v-if="editMode" :class="$style.feedPostActions">
                <Button
                    button-style="secondary"
                    accent-color="secondary"
                    @click="$emit('edit', feedPost)"
                >Upravit</Button>

                <Button
                    button-style="secondary"
                    accent-color="secondary"
                    @click="$emit('delete', feedPost)"
                    :style="{ '--color': 'var(--color-error)' }"
                >Smazat</Button>
            </div>

            <div :class="$style.feedPostHeader">
                <div :class="$style.feedPurpose" :style="{ backgroundColor: `var(${feedPost.background})` }">
                    <p>{{ feedPost.purposeLabel }}</p>
                </div>
                <div :class="$style.feedTimestamp">{{ timeAgoString(feedPost.createdAt) }}</div>
            </div>

            <div v-if="feedPost.author" :class="$style.feedPostAuthor">
                <Avatar
                    :name="feedPost.author.fullName"
                    :src="feedPost.author.pictureUrl ?? null"
                    :size="32"
                />
                <p :class="$style.authorName">{{ feedPost.author.fullName }}</p>
            </div>

            <div :class="$style.feedPostContent">
                <p>{{ feedPost.message }}</p>
            </div>
        </div>
    </div>
</template>

<style module lang="scss">
.feedPostWrapper {
    display: flex;
    position: relative;
    border: 1px solid color-mix(in srgb, var(--text-color-secondary) 20%, transparent 40%);
    border-left: none;
    border-radius: 12px;
    overflow: hidden;

    &:hover {
        .feedPostActions {
            opacity: 1;
            pointer-events: auto;
            transform: translateY(0);
        }
    }

    .feedPostLeft {
        display: flex;
        justify-content: center;
        padding: 16px;

        .iconWrapper {
            display: flex;
            justify-content: center;
            align-items: center;
            border-radius: 48px;
            height: 48px;
            width: 48px;

            .feedPostIcon {
                height: 24px;
                aspect-ratio: 1/1;
                mask-size: cover;
                mask-position: center;
                mask-repeat: no-repeat;
                background-color: var(--background-color);
            }
        }
    }

    .feedPostRight {
        display: flex;
        flex-direction: column;
        padding: 16px 12px;
        gap: 8px;

        .feedPostHeader {
            display: flex;
            align-items: center;
            gap: 8px;

            .feedPurpose {
                display: flex;
                align-items: center;
                padding: 4px 8px;
                border-radius: 24px;

                p {
                    margin: 0;
                    color: var(--background-color-primary);
                }
            }

            .feedTimestamp {
                font-size: 14px;
                color: var(--text-color-secondary);
                margin: 0;
            }
        }

        .feedPostAuthor {
            display: flex;
            align-items: center;
            gap: 12px;

            .authorName {
                margin: 0;
                font-size: 18px;
                font-weight: 700;
            }
        }

        .feedPostContent {
            p {
                margin: 0;
            }
        }

        .feedPostActions {
            position: absolute;
            top: 12px;
            right: 12px;

            display: flex;
            gap: 8px;

            opacity: 0;
            pointer-events: none;
            transform: translateY(-6px);
            transition: opacity 0.2s ease, transform 0.2s ease;

            :global(button) {
                padding: 6px 14px;
                font-size: 14px;
            }
        }
    }
}
</style>
