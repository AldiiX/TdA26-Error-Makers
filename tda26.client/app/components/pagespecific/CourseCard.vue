<script setup lang="ts">
    import type { Course } from "#shared/types";
    import Button from "~/components/Button.vue";
    import timeAgoString from "#shared/utils/timeAgoString";
    import type { Account, Lecturer } from "#shared/types";
    import getBaseUrl from "#shared/utils/getBaseUrl";
    import { NuxtLink } from "#components";

    const props = defineProps<{ 
        course: Course,
        editMode?: boolean
    }>();
    
    const emit = defineEmits<{
        (e: "edit"): void;
        (e: "delete"): void;
    }>();
    
    const { data: _account } = await useFetch<Account>(getBaseUrl() + `/api/v2/accounts/${props.course.lecturerUuid}`);
    const account = computed(() => _account.value ?? null);

    const lecturerDisplayName = computed(() => {
        const acc = account.value;
        if (!acc) return null;

        return acc.firstName && acc.lastName
            ? `${acc.firstName} ${acc.lastName}`
            : acc.username;
    });
</script>

<template>
    <div :class="$style.container">
        <div :class="$style.top">
            <NuxtLink :to="`/courses/${course.uuid}`" :class="$style.imageContainer">
                <div :class="$style.image"></div>                
            </NuxtLink>
        </div>
        <div :class="$style.bottom">
            <div :class="$style.infoContainer">
                <h1 :class="[$style.nadpis, 'text-gradient']" :title="course.name"> {{ course.name }}</h1>
                <p :class="$style.autor"> {{ lecturerDisplayName }}</p>
                <div :class="$style.date">
                    <p :class="$style.created">Vytvořeno {{ timeAgoString(course.createdAt) }}</p>
                    <p :class="$style.lastUpdate">Poslední úprava {{ timeAgoString(course.updatedAt) }}</p>
                </div>
            </div>
            <div :class="$style.buttonsContainer">
                <div :class="$style.anotherInfo">
                    <div :class="$style.info">
                        <div style="mask-image: url(/icons/star.svg)"></div>
                        <p>{{ }} 6</p>
                    </div>
                    <div :class="$style.info">
                        <div style="mask-image: url(/icons/views.svg)"></div>
                        <p>{{ }} 5</p>
                    </div>
                </div>

                <div :class="$style.actionContainer">
                    <div v-if="!editMode" :class="$style.userButtons">
                        <NuxtLink :to="`/courses/${course.uuid}`" :class="$style.button">
                            <Button button-style="primary" accent-color="secondary" style="width: 100%">Začít</Button>
                        </NuxtLink>
                    </div>
                    <div v-else :class="$style.lecturerButtons">
                        <Button button-style="primary" accent-color="secondary" @click="emit('edit')" style="width: 100%">Upravit</Button>
                        <Button button-style="secondary" accent-color="secondary" @click="emit('delete')" style="width: 100%">Smazat</Button>
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
    
    .top{
        width: 100%;
        
        .imageContainer{
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
            
            .image{
                
            }
        }
    }

    .bottom{
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        width: 100%;
        flex-grow: 1;
        padding: 16px;
        
        .infoContainer{
            display: flex;
            flex-direction: column;   

            h1{
                margin: 0;
                font-size: 24px;
                text-overflow: ellipsis;
                overflow: hidden;
                white-space: nowrap;
                padding-bottom: 3px;
            }

            .autor{
                font-size: 16px;
                color: var(--text-color-secondary);
                margin: 0 0 8px;
                font-weight: 600;
            }

            .date{
                .created, .lastUpdate {
                    font-size: 14px;
                    color: var(--text-color-secondary);
                    margin: 2px 0;
                }
            }
        }
        
        .buttonsContainer{
            display: flex;
            justify-content: space-between;
            align-items: center;
            width: 100%;

            .anotherInfo{
                display: flex;
                gap: 16px;

                .info {
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    gap: 4px;

                    >div {
                        mask-image: url("../../../public/icons/views.svg");
                        mask-size: cover;
                        mask-position: center;
                        mask-repeat: no-repeat;
                        height: 16px;
                        width: 16px;
                        background-color: var(--text-color-secondary);
                    }

                    >p {
                        font-size: 16px;
                        margin: 0;
                        color: var(--text-color);
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
</style>
