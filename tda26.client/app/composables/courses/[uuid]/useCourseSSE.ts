import {onBeforeUnmount, onMounted} from "vue";
import getBaseUrl from "#shared/utils/getBaseUrl";
import type {Course} from "#shared/types";
import { push } from "notivue";

export default function(params: {
    course: Ref<Course>;
    courseFullData: Ref<Course | null>;
    editMode: boolean;
}) {
    // propy
    let eventSource: EventSource | null = null;

    // funkce
    function onStatusChanged(event: MessageEvent) {
        const data = JSON.parse(event.data);

        if (data.status !== params.course.value.status && !params.editMode) {
            const promise = push.promise({
                title: "Upozornění",
                message: `Kurz změnil status na ${data.status}. Stránka se obnovuje...`,
                duration: Infinity
            });

            setTimeout(() => {
                window.location.reload();
            }, 1000);
        }
    }

    async function refreshFullCourse(fullCourse: Course, field: "materials" | "quizzes" | "modules") {
        try {
            const updatedCourse = await $fetch<Course>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}`,
                { query: { full: true } }
            );
            if (updatedCourse?.[field]) {
                (fullCourse as any)[field] = updatedCourse[field];
            }
        } catch (e) {
            console.error(`Failed to refresh ${field} after visibility change`, e);
        }
    }

    async function onMaterialVisibilityChanged(event: MessageEvent) {
        const data: { materialUuid: string; isVisible: boolean } = JSON.parse(event.data);
        const fullCourse = params.courseFullData.value;
        if (!fullCourse) return;

        if (!data.isVisible) {
            // hide: update isVisible in place so v-if hides it for non-owners
            const material = fullCourse.materials?.find(m => m.uuid === data.materialUuid);
            if (material) {
                material.isVisible = false;
            }
        } else {
            // show: re-fetch the full course to get the newly visible material
            await refreshFullCourse(fullCourse, "materials");
        }
    }

    async function onQuizVisibilityChanged(event: MessageEvent) {
        const data: { quizUuid: string; isVisible: boolean } = JSON.parse(event.data);
        const fullCourse = params.courseFullData.value;
        if (!fullCourse) return;

        if (!data.isVisible) {
            // hide: update isVisible in place so v-if hides it for non-owners
            const quiz = fullCourse.quizzes?.find(q => q.uuid === data.quizUuid);
            if (quiz) {
                quiz.isVisible = false;
            }
        } else {
            // show: re-fetch the full course to get the newly visible quiz
            await refreshFullCourse(fullCourse, "quizzes");
        }
    }

    async function onModuleVisibilityChanged(event: MessageEvent) {
        const data: { moduleUuid: string; isVisible: boolean } = JSON.parse(event.data);
        const fullCourse = params.courseFullData.value;
        if (!fullCourse) return;

        if (!data.isVisible) {
            // hide: update isVisible in place so the module is hidden for non-owners
            const mod = fullCourse.modules?.find(m => m.uuid === data.moduleUuid);
            if (mod) {
                mod.isVisible = false;
            }
        } else {
            // show: re-fetch modules to get the newly visible module
            await refreshFullCourse(fullCourse, "modules");
        }
    }

    // ostantni
    onMounted(() => {
        if (!import.meta.client || !params.course.value.uuid) return;

        const url = `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/stream`;

        eventSource = new EventSource(url, {
            withCredentials: true
        });

        eventSource.addEventListener("status_changed", onStatusChanged);
        eventSource.addEventListener("material_visibility_changed", onMaterialVisibilityChanged);
        eventSource.addEventListener("quiz_visibility_changed", onQuizVisibilityChanged);
        eventSource.addEventListener("module_visibility_changed", onModuleVisibilityChanged);

        eventSource.onerror = (err) => {
            console.error("SSE feed error", err);
        };

        eventSource.onopen = () => {
            //console.log("SSE connection opened");
        }
    });

    onBeforeUnmount(() => {
        if (eventSource) {
            eventSource.close();
            eventSource = null;
        }
    });

    return {};
}