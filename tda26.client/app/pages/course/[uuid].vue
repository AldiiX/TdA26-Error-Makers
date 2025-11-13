<script setup lang="ts">
    import { type Course } from "#shared/types";
    import getBaseUrl from "#shared/utils/getBaseUrl";

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
</script>

<template>
    <Head>
        <Title>Kurz • Think different Academy</Title>
    </Head>

    <p>{{ course.name }}</p>
    <p>{{ course.description }}</p>
    <p>{{ course.uuid }}</p>
</template>

<style module lang="scss">

</style>