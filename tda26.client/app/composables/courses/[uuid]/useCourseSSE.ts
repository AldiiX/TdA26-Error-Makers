import {onBeforeUnmount, onMounted} from "vue";
import getBaseUrl from "#shared/utils/getBaseUrl";
import type {Course} from "#shared/types";
import { push } from "notivue";

export default function(params: {
    course: Ref<Course>;
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

    function onMaterialVisibilityChanged(event: MessageEvent) {
        const data = JSON.parse(event.data);
        
        console.log(data);
    }

    function onQuizVisibilityChanged(event: MessageEvent) {
        const data = JSON.parse(event.data);

        console.log(data);
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