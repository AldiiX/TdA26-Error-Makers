<script setup lang="ts">
    import { type Lecturer } from "#shared/types";
    import getLecturerDisplayName from "#shared/utils/getLecturerDisplayNameHTML";
    import getBaseUrl from "#shared/utils/getBaseUrl";
    import Avatar from "~/components/Avatar.vue";
    import BlurText from "~/components/BlurText.vue";

    definePageMeta({
        layout: "normal-page-layout",
        alias: ["/lecturers/:uuid"],
        middleware: [
            defineNuxtRouteMiddleware(async (to) => {
                const uuid = to.params.uuid as string;
                //console.log(uuid);

                // pokud chybi uuid
                if (!uuid) {
                    return navigateTo("/lecturers");
                }

                // pokud je stranka /lecturers/:uuid, perm presmeruje na /lecturer/:uuid
                if (to.path.startsWith("/lecturers/")) {
                    return navigateTo(`/lecturer/${uuid}`);
                }

                try {
                    const lecturer = await $fetch<Lecturer>(getBaseUrl() + `/api/v2/lecturers/${uuid}`);
                    const key = `lecturer-${uuid}`;
                    const state = useState<Lecturer | null>(key, () => null);
                    state.value = lecturer;
                } catch (err: any) {

                    if (err.statusCode === 404) {
                        throw createError({
                            statusCode: 404,
                            statusMessage: `Lektor s tímto UUID nebyl nalezen.`,
                            /*data: {
                                message: `Lektor s UUID ${uuid} nebyl nalezen.`
                            }*/
                        });
                    }

                    return navigateTo("/lecturers");
                }
            })
        ]
    });

    const route = useRoute();
    const uuid = route.params.uuid as string;

        const key = `lecturer-${uuid}`;
    const lecturer = useState<Lecturer>(key);

    const lecturerName = computed(() => getLecturerDisplayName(lecturer.value!));

    const doesContactExist = computed(() => {
        return (lecturer.value?.emails && lecturer.value.emails.length > 0) ||
               (lecturer.value?.mobileNumbers && lecturer.value.mobileNumbers.length > 0);
    });

    // Dynamic SEO for lecturer page
    const fullName = computed(() => 
        `${lecturer.value?.titleBefore ? lecturer.value.titleBefore + ' ' : ''}${lecturer.value?.firstName || ''} ${lecturer.value?.middleName || ''} ${lecturer.value?.lastName || ''}${lecturer.value?.titleAfter ? ', ' + lecturer.value.titleAfter : ''}`.trim()
    );
    
    useSeo({
        title: fullName.value,
        description: lecturer.value?.bio || `Profil lektora ${fullName.value} na Think Different Academy. Zjistěte více o kurzech a zkušenostech.`,
        keywords: `lektor, ${fullName.value}, učitel, instruktor`,
        type: 'article'
    });
</script>

<template>
    <div :class="$style.profile">
        <div :class="$style.left">
            <div :class="$style.name">
                <p :class="$style.title">{{ lecturer.titleBefore }}</p>

                <h1 :class="['text-gradient']">{{ lecturer.firstName }} {{ lecturer.middleName }} {{ lecturer.lastName }}</h1>
                <p :class="$style.title">{{ lecturer.titleAfter }}</p>
            </div>

            <p :class="$style.bio">{{ lecturer.bio }}</p>

            <div :class="$style.elements">
                <div :class="[$style.el, 'liquid-glass']" v-if="doesContactExist">
                    <h3 :class="[/*'text-gradient'*/]">Kontakt</h3>

                    <div :class="$style.duo" v-if="lecturer.mobileNumbers?.length > 0">
                        <div :class="$style.icon" style="--ico: url(/icons/phone.svg)"></div>
                        <div>
                            <p v-for="phone in lecturer.mobileNumbers">{{ phone }}</p>
                        </div>
                    </div>

                    <div :class="$style.duo" v-if="lecturer.emails?.length > 0">
                        <div :class="$style.icon" style="--ico: url(/icons/email.svg)"></div>
                        <div>
                            <p v-for="email in lecturer.emails">{{ email }}</p>
                        </div>
                    </div>
                </div>

                <div :class="[$style.el, 'liquid-glass']">
                    <h3 :class="[/*'text-gradient'*/ $style.ac2]">Informace</h3>

                    <div :class="$style.duo">
                        <div :class="$style.icon" style="--ico: url(/icons/coin.svg)"></div>
                        <p>{{ lecturer.pricePerHour }} Kč/h</p>
                    </div>

                    <div :class="$style.duo">
                        <div :class="$style.icon" style="--ico: url(/icons/pin.svg)"></div>
                        <p>{{ lecturer.location }}</p>
                    </div>

                    <div :class="$style.duo">
                        <div :class="$style.icon" style="--ico: url(/icons/user_circle.svg)"></div>
                        <p>{{ new Date(lecturer.createdAt).toLocaleDateString() }}</p>
                    </div>
                </div>

                <div :class="[$style.el, 'liquid-glass']" v-if="lecturer.tags?.length > 0">
                    <h3 :class="[/*'text-gradient'*/]">Tagy</h3>

                    <div :class="$style.tags">
                        <p :class="$style.tag" v-for="tag in lecturer.tags">{{ tag }}</p>
                    </div>
                </div>
            </div>
        </div>

        <Avatar :class="$style.avatar" :name="lecturer.firstName + ' ' + lecturer.lastName" :src="lecturer.pictureUrl" :size="400" />
    </div>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.profile {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: space-between;
    gap: 2rem;
    margin-top: 5vh;
    margin-bottom: 10vh;
    animation: dfdkfospdgfdjdisjfsdapononbfdnvd 0.5s ease forwards;

    @keyframes dfdkfospdgfdjdisjfsdapononbfdnvd {
        from {
            opacity: 0;
            transform: translateY(32px);
            filter: blur(12px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
            filter: blur(0);
        }
    }

    @media (min-width: 768px) {
        flex-direction: row;
        align-items: flex-start;
    }

    > .left {
        display: grid;
        width: 60%;

        > .name {
            display: grid;

            p, h1 {
                margin: 0;
                font-size: 92px;
                font-weight: bold;
                width: fit-content;
            }

            p {
                font-size: 48px;
            }
        }

        .bio {
            font-size: 18px;
        }
    }

    .elements {
        display: flex;
        gap: 24px;
        margin-top: 2rem;

        @media (max-width: 767px) {
            flex-direction: column;
            gap: 2rem;
        }

        .tags {
            display: flex;
            flex-wrap: wrap;
            gap: 4px;
            flex-shrink: 1;

            .tag {
                background-color: var(--background-color-3);
                padding: 8px 16px;
                border-radius: 999px;
                margin: 0;
                font-size: 14px;
            }
        }

        > .el {
            display: flex;
            flex-direction: column;
            gap: 6px;
            padding: 24px;
            border-radius: 16px;
            box-shadow: inset 0 0 48px rgb(from var(--background-color-secondary) r g b/0.6), 0 0 8px rgba(0, 0, 0, 0.04);

            h3 {
                margin: 0;
                font-size: 24px;
                margin-bottom: 8px;
                width: fit-content;
                color: var(--accent-color-primary);

                &:is(.ac2) {
                    color: var(--accent-color-secondary-theme);
                }
            }

            > .duo {
                display: grid;
                grid-template-columns: auto 1fr;
                gap: 8px;
                align-items: start;

                > .icon {
                    width: 14px;
                    height: 14px;
                    background-color: var(--text-color-secondary);
                    mask-image: var(--ico);
                    mask-size: contain;
                    mask-repeat: no-repeat;
                    mask-position: center;
                    margin-top: 2px;
                    box-sizing: content-box;
                }

                p {
                    margin: 0;
                    width: max-content;
                }
            }
        }
    }
}

@media screen and (max-width: 1500px) {
    .profile {
        >.left >.name {
            p, h1 {
                font-size: 64px;
            }

            p {
                font-size: 32px;
            }
        }
        
        >.avatar {
            max-width: 350px;
        }
    }
}

@media screen and (max-width: 1200px) {
    .profile {
        flex-direction: column;
        align-items: center;
        gap: 2rem;
        justify-content: center;
        width: 100%;
        margin-top: -50px;

        >.left {
            width: 70%;
            order: 1;

            >.name {
                p, h1 {
                    font-size: 64px;
                }

                p {
                    font-size: 32px;
                }
            }
        }

        >.avatar {
            order: 0;
            max-width: 350px;
        }

        .elements {
            flex-direction: column;
            gap: 2rem;
        }
    }
}

/* Laptop */
@media screen and (max-width: app.$laptopBreakpoint) {
}

/* Tablet */
@media screen and (max-width: app.$tabletBreakpoint) {
    .profile {
        >.left {
            >.name {
                p, h1 {
                    font-size: 48px;
                }

                p {
                    font-size: 24px;
                }
            }
        }
    }
}

/* Mobile */
@media screen and (max-width: app.$mobileBreakpoint) {
    .profile {
        >.left {
            width: 100%;

            >.name {
                p, h1 {
                    font-size: 32px;
                }

                p {
                    font-size: 18px;
                }
            }
        }

        >.avatar {
            max-width: 250px;
        }
    }
}
</style>