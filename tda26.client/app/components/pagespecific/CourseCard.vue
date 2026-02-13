<script setup lang="ts">
import {computed, ref, watch} from "vue";
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
    (e: "delete"): void;
}>();

const loggedAccount = useState<Account | null>("loggedAccount");

const lecturerDisplayName = computed(() => {
    const lecturer = props.course.lecturer;
    const account = props.course.account;

    if(account && !lecturer) {
        return account.username;
    }

    if (!lecturer) return null;

    return lecturer.firstName && lecturer.lastName
        ? `${lecturer.firstName} ${lecturer.lastName}`
        : lecturer.username;
});

const revealStyle = computed(() => {
    return {
        "--reveal-delay-ms": `${props.revealDelayMs ?? 0}ms`
    } as Record<string, string>;
});

const isUploading = ref(false);
const uploadStatusText = ref("Nahrávám obrázek...");
const displayedImageUrl = ref(props.course.imageUrl);

watch(() => props.course.imageUrl, (value) => {
    displayedImageUrl.value = value;
});

const editBgImage = () => {
    if (isUploading.value) return;

    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = async () => {
        if (input.files && input.files[0]) {
            const formData = new FormData();
            formData.append('image', input.files[0]);
            isUploading.value = true;
            uploadStatusText.value = "Nahrávám obrázek...";

            try {
                const response = await fetch(`/api/v2/courses/${props.course.uuid}/image`, {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) return;

                displayedImageUrl.value = `/api/v2/courses/${props.course.uuid}/image?t=${Date.now()}`;
            } finally {
                isUploading.value = false;
            }
        }
    }
    
    input.click();
}

const resetBgImage = async () => {
    if (isUploading.value) return;

    isUploading.value = true;
    uploadStatusText.value = "Odstraňuji obrázek...";

    try {
        const response = await fetch(`/api/v2/courses/${props.course.uuid}/image`, {
            method: 'DELETE'
        });

        if (!response.ok) return;

        displayedImageUrl.value = null;
    } finally {
        isUploading.value = false;
    }
}
</script>

<template>
    <div :class="[$style.container, editMode && $style.editMode]" :style="revealStyle">
        <div :class="$style.top">
            <div v-if="isUploading" :class="$style.uploadOverlay">
                <div :class="$style.spinner"></div>
                <p>{{ uploadStatusText }}</p>
            </div>
            <div 
                :class="$style.editOverlay"
                v-if="editMode"
            >
                <div
                    :class="$style.bgButton"
                >
                    <span
                        @click="editBgImage"
                        :class="[$style.edit, isUploading && $style.disabled]"
                        title="Změnit obrázek kurzu"
                    ></span>
                    <span
                        @click="resetBgImage"
                        :class="[$style.reset, isUploading && $style.disabled]"
                        title="Obnovit výchozí obrázek kurzu"
                    ></span>
                </div>
            </div>
            <NuxtLink :to="`/courses/${course.uuid}`" :class="$style.imageContainer">
                <div :class="$style.image" v-if="displayedImageUrl" :style="{ '--bg': `url(${displayedImageUrl})` }"></div>

                <template v-if="!displayedImageUrl">
                    <div :class="$style.blob1"></div>
                    <div :class="$style.blob2"></div>
                    <div :class="$style.blob3"></div>
                    <div :class="$style.blob4"></div>
                    <div :class="$style.blob5"></div>

                    <div :class="$style.circle">
                        <div :class="$style.icon" :style="{ maskImage: `url(${course.imageUrlOrDefault})` }"></div>
                    </div>
                </template>
            </NuxtLink>
        </div>
        <div :class="$style.bottom">
            <div :class="$style.infoContainer">
                <h1 :class="[$style.nadpis, 'text-gradient']" :title="course.name">
                    {{ course.name }}
                </h1>

                <div :class="$style.authorAndRatingScore">
                    <NuxtLink
                            v-if="course.lecturer || course.account"
                            :class="[$style.author, { [$style.clickable]: course.lecturer }]"
                            :to="course.lecturer ? `/lecturers/${course.lecturer?.uuid}` : ''"
                    >
                        <Avatar
                                :class="$style.avatar"
                                :name="lecturerDisplayName ?? ''"
                                :src="course.lecturer?.pictureUrl ? course.lecturer.pictureUrl : null"
                        />
                        <p :class="$style.text">
                            {{ course?.lecturer?.fullNameWithoutTitles ?? course?.account?.fullNameWithoutTitles }}
                            <span v-if="course?.account?.uuid === loggedAccount?.uuid" :class="$style.you">(vy)</span>
                        </p>
                    </NuxtLink>

                    <div :class="$style.rating">
                        <div
                            v-for="n in 5"
                            :key="n"
                            :class="[
                                $style.star,
                                course.ratingScore >= n * 2 ? $style.full : course.ratingScore === n * 2 - 1 ? $style.half : null
                            ]"
                        >
                        </div>
                    </div>
                </div>


                <div :class="$style.date">
                    <p :class="$style.created">Vytvořeno {{ timeAgoString(course.createdAt) }}</p>
                    <p :class="$style.lastUpdate">Poslední úprava {{ timeAgoString(course.updatedAt) }}</p>
                </div>
            </div>
            <div :class="$style.buttonsContainer">
                <div :class="$style.anotherInfo">
                    <div :class="$style.info">
                        <div style="mask-image: url(/icons/thumbs_up_filled.svg)"></div>
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
                                @click="navigateTo(`/courses/${course.uuid}?edit=true`)"
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
@use "@/assets/variables" as app;

.liquid-glass {
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.75), 0 4px 30px rgba(0, 0, 0, 0.15);
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    backdrop-filter: blur(8px) saturate(1.6);
}

.editOverlay {
    position: absolute;
    top: 16px;
    right: 16px;
    z-index: 2;

    .bgButton {
        .edit {
            display: flex;
            align-items: center;
            justify-content: center;
            width: 32px;
            height: 32px;
            border-radius: 50%;
            background-color: rgba(255, 255, 255, 0.3);
            transition-duration: 0.3s;
            cursor: pointer;
            user-select: none;
            transition: background-color 0.3s;
            @extend .liquid-glass;

            &:hover {
                background-color: rgba(255, 255, 255, 0.7);
                transition-duration: 0.3s;
            }

            &::before {
                content: '';
                display: block;
                width: 20px;
                height: 20px;
                background-color: black;
                mask-size: contain;
                mask-position: center;
                mask-repeat: no-repeat;
                mask-image: url("/icons/imageEdit.svg");
            }
        }
        
        &:hover .reset {
            opacity: 1;
        }
        
        .reset {
            opacity: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            width: 32px;
            height: 32px;
            border-radius: 50%;
            background-color: rgba(255, 255, 255, 0.3);
            transition-duration: 0.3s;
            cursor: pointer;
            user-select: none;
            margin-top: 8px;
            transition: all 0.3s;
            @extend .liquid-glass;

            &:hover {
                background-color: rgba(255, 255, 255, 0.7);
                transition-duration: 0.3s;
            }

            &::before {
                content: '';
                display: block;
                width: 20px;
                height: 20px;
                background-color: black;
                mask-size: contain;
                mask-position: center;
                mask-repeat: no-repeat;
                mask-image: url("/icons/trash.svg");
            }
        }
    }
}

.container {
    display: flex;
    flex-direction: column;
    align-items: center;
    height: 400px;
    width: 350px;
    border-radius: 24px;
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
        position: relative;
        width: 100%;

        .uploadOverlay {
            position: absolute;
            inset: 0;
            z-index: 3;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 8px;
            border-radius: 24px;
            background-color: rgba(0, 0, 0, 0.35);
            color: white;
            pointer-events: all;

            p {
                margin: 0;
                font-size: 14px;
                font-weight: 600;
            }

            .spinner {
                width: 28px;
                height: 28px;
                border-radius: 50%;
                border: 3px solid rgba(255, 255, 255, 0.35);
                border-top-color: white;
                animation: spin 0.9s linear infinite;
            }
        }

        .imageContainer {
            display: block;
            min-height: 200px;
            width: 100%;
            background: linear-gradient(160deg, var(--accent-color-primary), var(--accent-color-primary-darker));
            overflow: hidden;
            border-radius: 24px;
            transition: filter 0.3s;
            position: relative;

            >* {
                pointer-events: none;
            }

            &:hover {
                filter: brightness(0.75);
                transition-duration: 0.3s;
            }

            .image {
                width: 100%;
                height: 100%;
                background-image: var(--bg);
                background-size: cover;
                background-position: center;
                position: absolute;
            }

            .blob1 {
                width: 32px;
                aspect-ratio: 1/1;
                position: absolute;
                top: 16%;
                left: 10%;
                background: white;
                opacity: 0.1;
                mask: linear-gradient(to bottom right, black, transparent);
                border-radius: 50%;
            }

            .blob2 {
                width: 64px;
                aspect-ratio: 1/1;
                position: absolute;
                bottom: 8%;
                left: 6%;
                background: black;
                opacity: 0.1;
                mask: linear-gradient(to bottom right, black, transparent);
                border-radius: 50%;
            }

            .blob3 {
                width: 24px;
                aspect-ratio: 1/1;
                position: absolute;
                top: 12%;
                right: 6%;
                background: black;
                opacity: 0.1;
                mask: linear-gradient(to bottom right, black, transparent);
                border-radius: 50%;
            }

            .blob4 {
                width: 92px;
                aspect-ratio: 1/1;
                position: absolute;
                bottom: 12%;
                right: -6%;
                background: white;
                opacity: 0.1;
                mask: linear-gradient(206deg, black, transparent);
                border-radius: 50%;
            }

            .blob5 {
                width: 48px;
                aspect-ratio: 1/1;
                position: absolute;
                bottom: 2%;
                right: 12%;
                background: black;
                opacity: 0.1;
                mask: linear-gradient(36deg, black, transparent);
                border-radius: 50%;
            }

            .circle {
                position: absolute;
                width: calc(64px + 40px);
                aspect-ratio: 1/1;
                top: 50%;
                left: 50%;
                transform: translate(-50%, -50%);
                border-radius: 50%;
                background: linear-gradient(135deg, var(--accent-color-secondary-transparent-03), var(--accent-color-secondary-transparent-01));
                box-shadow: 0 0 32px rgba(0, 0, 0, 0.1);

                .icon {
                    position: absolute;
                    width: 50%;
                    height: 50%;
                    top: 50%;
                    left: 50%;
                    transform: translate(-50%, -50%);
                    background-color: var(--accent-color-secondary-transparent-06);
                    mask-size: contain;
                    mask-position: center;
                    mask-repeat: no-repeat;
                    //mask-image: url(/icons/courseicons/paint.svg);
                }
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
                font-size: 20px;
                text-overflow: ellipsis;
                overflow: hidden;
                white-space: nowrap;
                padding-bottom: 3px;
            }

            .authorAndRatingScore {
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin: 8px 0;

                .author {
                    display: flex;
                    align-items: center;
                    gap: 8px;
                    margin: 0;
                    text-decoration: none;
                    transition-duration: 0.3s;
                    width: fit-content;

                    &:is(.clickable) {
                        &:hover {
                            opacity: 0.5;
                            transition-duration: 0.3s;
                        }
                    }


                    .text {
                        font-size: 16px;
                        color: var(--text-color-secondary);
                        font-weight: 600;
                        margin: 0;

                        .you {
                            font-size: 14px;
                            color: var(--accent-color-secondary-theme);
                            margin-left: 4px;
                        }
                    }

                    .avatar {
                        --size: 24px !important;
                    }
                }

                .rating {
                    display: flex;
                    gap: 4px;

                    .star {
                        width: 16px;
                        aspect-ratio: 1/1;
                        mask: url(/icons/star.svg);
                        background: var(--text-color-3);
                        mask-size: cover;
                        mask-position: center;
                        mask-repeat: no-repeat;

                        &:is(.half) {
                            background: linear-gradient(90deg, var(--accent-color-primary) 50%, var(--text-color-3) 50%);
                        }

                        &:is(.full) {
                            background: linear-gradient(90deg, var(--accent-color-primary) 50%, var(--accent-color-primary) 50%);
                        }
                    }
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

.disabled {
    pointer-events: none;
    opacity: 0.6;
}

@keyframes spin {
    to {
        transform: rotate(360deg);
    }
}

@media screen and (max-width: app.$mobileBreakpoint) {
    .bottom {
        gap: 8px;
        
        .buttonsContainer .anotherInfo {
            gap: 8px !important;
        }
    }
    
    .editMode {
        .buttonsContainer {
            flex-direction: column;
            align-items: start !important;
            gap: 8px !important;
            
            .actionContainer {
                width: 100% !important;
            }
        }
    }
}
</style>


