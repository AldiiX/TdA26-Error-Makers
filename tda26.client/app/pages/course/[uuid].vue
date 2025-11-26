<script setup lang="ts">
    import {type Course} from "#shared/types";
    import getBaseUrl from "#shared/utils/getBaseUrl";
    import { NuxtLink } from '#components';
    import Button from "~/components/Button.vue";
    declare const grecaptcha: any;

    definePageMeta({
        layout: "normal-page-layout",
        alias: ["/courses/:uuid"],
        middleware: [
            defineNuxtRouteMiddleware(async (to) => {
                const uuid = to.params.uuid as string;
                //console.log(uuid);

                // pokud chybi uuid
                if (!uuid) {
                    return navigateTo("/courses");
                }

                // pokud je stranka /courses/:uuid, perm presmeruje na /course/:uuid
                if (to.path.startsWith("/courses/")) {
                    return navigateTo(`/course/${uuid}`);
                }

                try {
                    const course = await $fetch<Course>(getBaseUrl() + `/api/v2/courses/${uuid}`);
                    const key = `course-${uuid}`;
                    const state = useState<Course | null>(key, () => null);
                    
                    state.value = course;
                } catch (err: any) {

                    if (err.statusCode === 404) {
                        throw createError({
                            statusCode: 404,
                            statusMessage: `Kurz s tímto UUID nebyl nalezen.`,
                            /*data: {
                                message: `Kurz s UUID ${uuid} nebyl nalezen.`
                            }*/
                        });
                    }

                    return navigateTo("/courses");
                }
            })
        ]
    });

    const route = useRoute();
    const uuid = route.params.uuid as string;

    const key = `course-${uuid}`;
    const course = useState<Course>(key);

    const menuItems = ['Materiály', 'Aktivita'];
    const selectedItem = ref(menuItems[0]);
    
    const selectItem = (item: string) => {
        selectedItem.value = item;
    };

    const getHostname = (url?: string) => {
        try {
            return url ? new URL(url).hostname : ''
        } catch {
            return ''
        }
    }

    // frontendove poslani view eventu
    onMounted(async () => {
        const captchaToken = await grecaptcha.execute(
            "6LfDQhksAAAAAEz_ujbJNian3-e-TfyKx8gzRaCL",
            { action: "submit" }
        );

        await fetch(`/api/v2/courses/${uuid}/view`, {
            method: 'POST',
            body: JSON.stringify({ token: captchaToken }),
            headers: {
                'Content-Type': 'application/json'
            }
        });
    })
</script>

<template>
    <Head>
        <Title>Kurz • Think different Academy</Title>
    </Head>


    <div :class="$style.course">
        <h1 :class="['text-gradient', $style.title]">{{ course.name }}</h1>
        <div :class="$style.info">
            <p>{{ course.description }}</p>
            <div :class="['liquid-glass', $style.brief]">
                <div :class="$style.fields">
                    <div :class="$style.el">
                        <p :class="$style.title">Počet zhlédnutí</p>
                        <p :class="$style.item">Samen</p>
                    </div>
                    <div :class="$style.el">
                        <p :class="$style.title">Hodnocení</p>
                        <p :class="$style.item">Samen</p>
                    </div>
                    <div :class="$style.el">
                        <p :class="$style.title">Něco dalšího</p>
                        <p :class="$style.item">Samen</p>
                    </div>
                </div>
                <div :class="$style.buttons">
                    <Button button-style="primary" accent-color="primary">Zapsat se</Button>
                    <Button button-style="secondary" accent-color="secondary">Více informací</Button>
                </div>
            </div>
        </div>
        
        <div :class="$style.details">
            <nav>
                <ul>
                    <li
                        v-for="item in menuItems"
                        :key="item"
                        :class="[item === selectedItem ? $style.active : '']"
                        @click="selectItem(item)">
                        {{ item }}
                    </li>
                </ul>
            </nav>
            <div :class="['liquid-glass']">
                <div v-if="selectedItem == 'Materiály'" :class="$style.materials">
                    <p v-if="course.materials.length == 0">Tento kurz nemá žádné materiály.</p>
                    <ul v-else>
                        <li v-for="material in course.materials" :key="material.uuid">
                            <!-- FILE MATERIAL -->
                            <template v-if="material.type === 'file'">
                                <NuxtLink :href="`/api/v2/courses/${course.uuid}/materials/${material.uuid}`" target="_blank" rel="noopener noreferrer">
                                    <div :class="$style.fileIcon"></div>
    
                                    <div :class="$style.fileInfo">
                                        <p>{{ material.name }}</p>
                                        <div :class="$style.fileDetails">
                                            <p>{{ material.fileUrl.match(/\.([^.]+)$/)?.[1]?.toUpperCase() ?? "JINÉ" }} • {{ new Date(material.createdAt).toLocaleDateString() }}</p>
                                        </div>
                                    </div>
                                    <p :class="$style.description">{{ material.description }}</p>
                                </NuxtLink>
                            </template>
    
                            <!-- URL MATERIAL -->
                            <template v-else-if="material.type === 'url'">
                                <NuxtLink :href="material.url" target="_blank" rel="noopener noreferrer">
                                    <div :class="$style.favicon">
                                        <img v-if="material.faviconUrl" :src="material.faviconUrl" alt="Favicon" />
                                    </div>
    
                                    <div :class="$style.fileInfo">
                                        <p>{{ material.name }}</p>
                                        <div :class="$style.fileDetails">
                                            <p>{{ getHostname(material.url) }} • {{ new Date(material.createdAt).toLocaleDateString() }}</p>
                                        </div>
                                    </div>
                                    <p :class="$style.description">{{ material.description }}</p>
                                </NuxtLink>
                            </template>
                        </li>
                    </ul>
                </div>
                <div v-if="selectedItem == 'Aktivita'" :class="$style.activity">
                    <p v-if="course.feed.length == 0">Žádná nedávná aktivita.</p>
                    <ul v-else>
                        <li v-for="feedPost in course.feed" :key="feedPost.uuid">
                            <!-- // TODO: -->
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
    
</template>

<style module lang="scss">
    .title{
        font-size: 72px;
        margin: 0;
        overflow: visible;
        width: fit-content;
    }
    
    ul {
        list-style: none;
        padding: 0;
        margin: 0;
    }
    
    .course {
        display: flex;
        flex-direction: column;
        gap: 20px;
        
        >.info {
            display: flex;
            justify-content: space-between;
            gap: 24px;

            >p {
                font-size: 18px;
                margin-bottom: 6px;
            }
            
            .brief {
                display: flex;
                flex-direction: column;
                min-width: 400px;
                gap: 24px;
                padding: 24px;
                border-radius: 16px;
                box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);

                >.fields {
                    display: flex;

                    >.el {
                        position: relative;
                        border-right: 1px solid color-mix(in srgb, var(--text-color-secondary) 30%, transparent 40%);
                        padding: 0 24px;


                        >.title {
                            font-size: 16px;
                            margin-bottom: 8px;
                        }
                        >.item {
                            font-size: 24px;
                            margin: 0;
                        }

                        &:last-child {
                            border-right: none;
                            padding-right: 0;
                        }

                        &:first-child {
                            padding-left: 0;
                        }
                    }
                }

                >.buttons {
                    display: flex;
                    gap: 12px;
                }
            }
        }
        
        
        .details {
            margin-top: 32px;
            
            nav {
                ul {
                    display: flex;
                    gap: 24px;
                    
                    li {
                        font-size: 18px;
                        padding-bottom: 8px;
                        cursor: pointer;
                        transition: all 0.2s;
                        user-select: none;
                        opacity: .6;
                        font-weight: 600;
                        
                        &:hover {
                            color: var(--accent-color-secondary-theme);
                            opacity: 1;
                        }


                        &::after {
                            content: '';
                            position: absolute;
                            bottom: 0;
                            left: 0;
                            width: 100%;
                            height: 2px;
                            background-color: transparent;
                            border-radius: 4px;
                            transition: all 0.3s;
                        }
                        
                        &.active {
                            border-bottom: none;
                            position: relative;
                            pointer-events: none;
                            opacity: 1;

                            &::after {
                                background-color: var(--accent-color-primary);
                            }
                        }
                    }
                }
            }
            
            >div {
                padding: 24px;
                border-radius: 16px;
                box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);
                margin-top: 16px;
            }

            .materials {
                ul {
                    display: flex;
                    flex-direction: column;
                    gap: 12px;
                    
                    li >a {
                        display: flex;
                        align-items: center;
                        gap: 6px;
                        //box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);
                        border: 1px solid color-mix(in srgb, var(--text-color-secondary) 10%, transparent 40%);
                        border-radius: 12px;
                        padding: 12px 16px;
                        transition: background-color 0.3s;
                        color: var(--text-color);
                        text-decoration: none;
                        
                        &:hover {
                            background-color: color-mix(in srgb, var(--accent-color-primary) 5%, var(--background-color-secondary) 95%);
                        }
                        
                        .fileIcon {
                            mask-image: url('../../../public/icons/file.svg');
                            mask-size: cover;
                            mask-position: center;
                            mask-repeat: no-repeat;

                            width: 24px;
                            height: 24px;
                            background-color: var(--text-color-secondary);
                            opacity: 0.6;
                        }
                        
                        .fileInfo {
                            display: flex;
                            flex-direction: column;
                            gap: 4px;
                            border-right: 1px solid color-mix(in srgb, var(--text-color-secondary) 20%, transparent 40%);
                            padding-right: 12px;

                            p {
                                margin: 0;
                                font-size: 16px;
                            }

                            .fileDetails >p {
                                font-size: 12px;
                                color: var(--text-color-secondary);
                            }
                        }
                        
                        .favicon img {
                            border-radius: 4px;
                            overflow: hidden;
                        }
                        
                        .description {
                            margin-left: 10px;
                            height: 100%;
                            font-size: 14px;
                            color: var(--text-color-secondary);
                        }
                    }
                }
            }
        }
    }
</style>