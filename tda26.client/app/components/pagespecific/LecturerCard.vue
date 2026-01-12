<script setup lang="ts">
import type { Lecturer } from "#shared/types";
import { computed } from "vue";
import Avatar from "~/components/Avatar.vue";
import { NuxtLink } from "#components";

const props = defineProps<{
    lecturer: Lecturer | null,
    style?: Record<string, string>,
    nameTextClass?: string,
}>();

const displayName = computed(() => {
    const tb = (props.lecturer as any).titleBefore ?? (props.lecturer as any).title_before ?? "";
    const ta = (props.lecturer as any).titleAfter ?? (props.lecturer as any).title_after ?? "";
    const first = (props.lecturer as any).firstName ?? (props.lecturer as any).first_name ?? "";
    const middle = (props.lecturer as any).middleName ?? (props.lecturer as any).middle_name ?? "";
    const last = (props.lecturer as any).lastName ?? (props.lecturer as any).last_name ?? "";

    const name = [first, middle, last].filter(Boolean).join(" ");
    const before = tb ? tb + "\u00A0" : "";
    const after = ta ? ", " + ta : "";
    return `<span>${before}</span>${name}<span>${after}</span>`.trim();
});

const profileUrl = computed(() => {
    if(!props.lecturer) return "#";

    const id = (props.lecturer as any).uuid ?? (props.lecturer as any).id;
    return `/lecturer/${id}`;
});

type AnyTag = { name: string; color?: string } | string;

const tags = computed(() => {
    if(!props.lecturer) return [] as AnyTag[];

    const raw = (props.lecturer as any).tags;
    if (!raw) return [] as { name: string }[];
    if (Array.isArray(raw)) return raw as any[];
    try { return JSON.parse(raw as unknown as string) as any[]; } catch { return []; }
});

const locAndPrice = computed(() => {
    if(!props.lecturer) return [] as string[];

    const city = (props.lecturer as any).location ?? "";
    const price = (props.lecturer as any).pricePerHour ?? (props.lecturer as any).price_per_hour ?? null;
    const arr: string[] = [];
    if (city) arr.push(String(city));
    if (price !== null && price !== undefined && price !== "") arr.push(`${price} Kč/h`);
    return arr;
});

const pictureStyle = computed(() => {
    const url = (props.lecturer as any).pictureUrl ?? (props.lecturer as any).picture_url ?? "";
    return url ? { backgroundImage: `url('${url}')` } : {};
});

const generateRandomNbspNumber = ( min: number, max: number ): string => {
    const n = Math.floor( Math.random() * ( max - min + 1 ) ) + min;
    return '\u00A0'.repeat( n );
};
</script>

<template>
    <NuxtLink :to="profileUrl" :class="[$style.vizitka, 'liquid-glass']" :style="style">
        <Avatar v-if="lecturer" :class="$style.avatar" :src="lecturer.pictureUrl" :alt="displayName" :size="140" :name="lecturer.firstName + ' ' + lecturer.lastName" />
        <div v-else :class="[$style.skeleton, $style.avatarloading]"></div>

        <div :class="$style.description">
            <h1 :class="[$style.title, 'text-gradient']" v-if="lecturer">
                <span :class="[$style.name, nameTextClass]" v-html="displayName"></span>
            </h1>

            <div :class="[$style.skeleton]" style="width: 60%; height: 43px; border-radius: 12px" v-else></div>

            <div v-if="!lecturer" :class="[$style.skeleton]" style="width: 80%; height: 18px; margin-top: 8px; border-radius: 12px"></div>
            <p v-else-if="(lecturer as any).claim" :class="$style.claim">{{ (lecturer as any).claim }}</p>

            <div style="display: grid; gap: 4px; margin-top: 16px" >
                <template v-if="lecturer">
                    <div v-if="tags.length" :class="$style.tags">
                        <span v-for="t in tags" :key="typeof t === 'string' ? t : t.name" :class="$style.tag">{{ typeof t === 'string' ? t : t.name }}</span>
                    </div>

                    <div v-if="locAndPrice.length" :class="$style.tags">
                        <div v-for="(v, i) in locAndPrice" :key="i" :class="$style.locpr">
                            <p>{{ v }}</p>
                        </div>
                    </div>
                </template>

                <template v-else>
                    <div :class="[$style.tags]">
                        <span v-for="t in 10" :class="[$style.tag, $style.skeleton]">{{ generateRandomNbspNumber(8, 32) }}</span>
                    </div>

                    <div :class="$style.tags">
                        <span v-for="t in 2" :class="[$style.tag, $style.skeleton]">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>
                    </div>
                </template>
            </div>
        </div>
    </NuxtLink>
</template>

<style module lang="scss">
@use "../../app" as app;

.vizitka {
    /* base layout */
    display: flex;
    align-items: start;
    gap: 20px;
    padding: 16px 24px;
    border-radius: 24px;
    text-decoration: none;
    color: inherit;
    width: 100%;
    //max-width: 900px;
    margin: 0 auto;
    background-color: rgb(from var(--background-color-secondary) r g b / 0.5);
    box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b / 0.6), 0 0 8px rgba(0, 0, 0, 0.04);
    transition: 0.3s;

    .skeleton {
        background-color: var(--background-color-3);
        animation: skeleton-loading 1.5s infinite;

        @keyframes skeleton-loading {
            0% {
                background-color: var(--background-color-3);
            }
            50% {
                background-color: var(--background-color-primary);
            }
            100% {
                background-color: var(--background-color-3);
            }
        }
    }

    &:hover {
        background-color: var(--background-color-secondary);
        transition: 0.3s;
    }

    &:nth-child(odd) {
        .locpr {
            background: var(--accent-color-primary);
            color: var(--accent-color-primary-text);
        }

        .avatar {
            background-color: var(--accent-color-primary) !important;
        }
    }

    .avatar {
        background-color: var(--accent-color-secondary-theme);
    }

    .avatarloading {
        width: 140px;
        height: 140px;
        border-radius: 100%;
        flex-shrink: 0;
    }

    .pfp {
        width: 140px;
        height: 140px;
        border-radius: 16px;
        background: linear-gradient(135deg, #111827, #0b1020);
        background-size: cover;
        background-position: center;
        flex: 0 0 auto;
        position: relative;
        overflow: hidden;
    }

    .initials {
        /* placeholder pro profilovku */
        position: absolute;
        inset: 0;
        display: grid;
        place-items: center;
        font-weight: 800;
        font-size: 44px;
        opacity: .6;
    }

    .description {
        width: unset;
        display: flex;
        flex-direction: column;
        justify-content: center;
        flex-grow: 1;
    }

    .title {
        display: flex;
        flex-direction: row;
        font-size: 32px;
        line-height: 1.1;
        margin: 0;
        width: fit-content;
    }

    .name {
        text-align: center;
        line-height: 43px;

        /*>span {
            color: var(--accent-color-secondary-theme);
        }*/
    }

    .claim {
        margin: 0;
        color: var(--text-color-secondary, #9aa4b2);
    }

    .tags {
        display: flex;
        flex-wrap: wrap;
        gap: 4px;
    }

    .tag {
        padding: 8px 16px;
        border-radius: 999px;
        font-size: 14px;
        background: var(--background-color-3);
    }

    .locpr {
        display: inline-flex;
        align-items: center;
        padding: 8px 16px;
        border-radius: 999px;
        background: var(--accent-color-secondary-theme);
        color: var(--accent-color-secondary-theme-text);

        p { margin: 0; font-size: 13px; }
    }
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
}

// 750px
@media screen and (max-width: 750px) {
    .vizitka {
        flex-direction: column;
        align-items: center;
        text-align: center;

        .description {
            align-items: center;
            margin-top: 12px;
        }

        .title {
            justify-content: center;
        }

        .tags {
            justify-content: center;
            
            &:first-child {
                margin-bottom: 8px;
            }
        }
    }
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
}

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
}
</style>
