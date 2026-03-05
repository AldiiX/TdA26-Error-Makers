import type { Course, CourseStatus } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";

/**
 * Otevře SSE stream pro každý kurz v `courses` listu a při status_changed
 * eventu aktualizuje status přímo v listu (bez refetche).
 */
export function useCourseListSSE(courses: Ref<Course[] | null>) {
    if (!import.meta.client) return;

    // uuid -> EventSource
    const sources = new Map<string, EventSource>();

    function openStream(course: Course) {
        if (sources.has(course.uuid)) return;

        const url = `${getBaseUrl()}/api/v1/courses/${course.uuid}/stream`;
        const es = new EventSource(url, { withCredentials: true });

        es.addEventListener("status_changed", (event: MessageEvent) => {
            const data: { status: CourseStatus } = JSON.parse(event.data);
            const list = courses.value;
            if (!list) return;

            const found = list.find(c => c.uuid === course.uuid);
            if (found && found.status !== data.status) {
                found.status = data.status;
            }
        });

        es.onerror = () => {
            // při chybě zavřeme a po chvíli zkusíme znovu
            es.close();
            sources.delete(course.uuid);
            setTimeout(() => {
                const current = courses.value?.find(c => c.uuid === course.uuid);
                if (current) openStream(current);
            }, 5000);
        };

        sources.set(course.uuid, es);
    }

    function closeAll() {
        for (const es of sources.values()) {
            es.close();
        }
        sources.clear();
    }

    // při každé změně listu kurzů otevři streamy pro nové
    watch(courses, (list) => {
        if (!list) return;
        for (const course of list) {
            openStream(course);
        }
    }, { immediate: true, deep: false });

    onBeforeUnmount(() => {
        closeAll();
    });
}
