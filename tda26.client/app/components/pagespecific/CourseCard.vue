<script setup lang="ts">
import { computed } from "vue";
import type { Course } from "#shared/types";
import Button from "~/components/Button.vue";
import timeAgoString from "#shared/utils/timeAgoString";
import type { Account } from "#shared/types";
import { NuxtLink } from "#components";
import Avatar from "~/components/Avatar.vue";

const props = defineProps<{
    course: Course,
    editMode?: boolean,
    /** delay reveal animace v ms */
    revealDelayMs?: number
}>();

const emit = defineEmits<{
    (e: "edit"): void;
    (e: "delete"): void;
}>();

const loggedUser = useState<Account | null>("loggedUser");

const lecturerDisplayName = computed(() => {
    const acc = props.course.lecturer;
    if (!acc) return null;

    return acc.firstName && acc.lastName
        ? `${acc.firstName} ${acc.lastName}`
        : acc.username;
});

const revealStyle = computed(() => {
    return {
        "--reveal-delay-ms": `${props.revealDelayMs ?? 0}ms`
    } as Record<string, string>;
});
</script>

<template>
    <div :class="$style.container" :style="revealStyle">
        <div :class="$style.top">
            <NuxtLink :to="`/courses/${course.uuid}`" :class="$style.imageContainer">
                <div :class="$style.image"></div>
            </NuxtLink>
        </div>
        <div :class="$style.bottom">
            <div :class="$style.infoContainer">
                <h1 :class="[$style.nadpis, 'text-gradient']" :title="course.name">
                    {{ course.name }}
                </h1>

                <NuxtLink
                        v-if="course.lecturer"
                        :class="$style.author"
                        :to="`/lecturers/${course.lecturer.uuid}`"
                >
                    <Avatar
                            :class="$style.avatar"
                            :name="lecturerDisplayName ?? ''"
                            :src="course.lecturer?.pictureUrl ? course.lecturer.pictureUrl : null"
                    />
                    <p :class="$style.text">
                        {{ course?.lecturer?.fullNameWithoutTitles }}
                        <span v-if="course?.lecturer?.uuid === loggedUser?.uuid">(vy)</span>
                    </p>
                </NuxtLink>

                <div :class="$style.date">
                    <p :class="$style.created">Vytvořeno {{ timeAgoString(course.createdAt) }}</p>
                    <p :class="$style.lastUpdate">Poslední úprava {{ timeAgoString(course.updatedAt) }}</p>
                </div>
            </div>
            <div :class="$style.buttonsContainer">
                <div :class="$style.anotherInfo">
                    <div :class="$style.info">
                        <div style="mask-image: url(/icons/star.svg)"></div>
                        <p>{{ course.likeCount }}</p>
                    </div>
                    <div :class="$style.info">
                        <div style="mask-image: url(/icons/views.svg)"></div>
                        <p>{{ course.viewCount }}</p>
                    </div>
                </div>

                <div :class="$style.actionContainer">
                    <div v-if="!editMode" :class="$style.userButtons">
                        <NuxtLink :to="`/courses/${course.uuid}`" :class="$style.button">
                            <Button button-style="primary" accent-color="secondary" style="width: 100%">
                                Zobrazit kurz
                            </Button>
                        </NuxtLink>
                    </div>
                    <div v-else :class="$style.lecturerButtons">
                        <Button
                                button-style="primary"
                                accent-color="secondary"
                                @click="emit('edit')"
                                style="width: 100%"
                        >
                            Upravit
                        </Button>
                        <Button
                                button-style="secondary"
                                accent-color="secondary"
                                @click="emit('delete')"
                                style="width: 100%"
                        >
                            Smazat
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style module lang="scss">

.liquid-glass {
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 4px 30px rgba(0, 0, 0, 0.15);
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    backdrop-filter: blur(8px) saturate(1.6);
}

.container {
    display: flex;
    flex-direction: column;
    align-items: center;
    height: 400px;
    width: 350px;
    border-radius: 16px;
    box-shadow: 0 0 32px rgba(0, 0, 0, 0.1);
    background-color: var(--background-color-secondary);

    @extend .liquid-glass;

    /* reveal animace */
    opacity: 0;
    transform: translateY(16px);
    filter: blur(6px);
    animation-name: course-card-reveal;
    animation-duration: 0.5s;
    animation-timing-function: ease-out;
    animation-fill-mode: forwards;
    animation-delay: var(--reveal-delay-ms, 0ms);

    .top {
        width: 100%;

        .imageContainer {
            display: block;
            min-height: 200px;
            width: 100%;
            background-color: var(--accent-color-primary);
            overflow: hidden;
            border-radius: 16px;
            transition: filter 0.3s;

            &:hover {
                filter: brightness(0.9);
                transition-duration: 0.3s;
            }

            .image {

            }
        }
    }

    .bottom {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        width: 100%;
        flex-grow: 1;
        padding: 16px;

        .infoContainer {
            display: flex;
            flex-direction: column;

            h1 {
                margin: 0;
                font-size: 24px;
                text-overflow: ellipsis;
                overflow: hidden;
                white-space: nowrap;
                padding-bottom: 3px;
            }

            .author {
                display: flex;
                align-items: center;
                gap: 8px;
                margin: 8px 0;
                text-decoration: none;
                transition-duration: 0.3s;
                width: fit-content;

                &:hover {
                    opacity: 0.5;
                    transition-duration: 0.3s;
                }

                .text {
                    font-size: 16px;
                    color: var(--text-color-secondary);
                    font-weight: 600;
                    margin: 0;
                }

                .avatar {
                    --size: 24px !important;
                }
            }

            .date {
                .created,
                .lastUpdate {
                    font-size: 14px;
                    color: var(--text-color-secondary);
                    margin: 2px 0;
                }
            }
        }

        .buttonsContainer {
            display: flex;
            justify-content: space-between;
            align-items: end;
            width: 100%;

            .anotherInfo {
                display: flex;
                gap: 16px;

                .info {
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    gap: 4px;

                    > div {
                        mask-size: cover;
                        mask-position: center;
                        mask-repeat: no-repeat;
                        height: 16px;
                        width: 16px;
                        background-color: var(--text-color-secondary);
                    }

                    > p {
                        font-size: 16px;
                        margin: 0;
                        color: var(--text-color-secondary);
                    }
                }
            }

            .button {
                width: 50%;
            }

            .actionContainer {
                .lecturerButtons {
                    display: flex;
                    gap: 8px;

                    button {
                        height: fit-content;
                    }
                }
            }
        }
    }
}

@keyframes course-card-reveal {
    to {
        opacity: 1;
        transform: translateY(0);
        filter: blur(0);
    }
}
</style>