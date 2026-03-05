import type { Course, CourseStatus } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";

/**
 * Otevře JEDEN globální SSE stream pro /api/v1/courses/stream a při
 * status_changed eventu aktualizuje status příslušného kurzu v listu
 * (bez refetche). Tím se zabrání otevírání jednoho streamu na každý kurz.
 */
export function useCourseListSSE(courses: Ref<Course[] | null>) {
    if (!import.meta.client) return;

    let es: EventSource | null = null;
    let retryTimer: ReturnType<typeof setTimeout> | null = null;

    function openStream() {
        if (es) return;

        const url = `${getBaseUrl()}/api/v1/courses/stream`;
        es = new EventSource(url, { withCredentials: true });

        es.addEventListener("status_changed", (event: MessageEvent) => {
            const parsed: { courseId: string; data: { status: CourseStatus } } = JSON.parse(event.data);
            const list = courses.value;
            if (!list) return;

            const found = list.find(c => c.uuid === parsed.courseId);
            if (found && found.status !== parsed.data.status) {
                found.status = parsed.data.status;
            }
        });

        es.onerror = () => {
            es?.close();
            es = null;
            // zkusit znovu za 5 sekund
            retryTimer = setTimeout(() => {
                retryTimer = null;
                openStream();
            }, 5000);
        };
    }

    function closeStream() {
        if (retryTimer !== null) {
            clearTimeout(retryTimer);
            retryTimer = null;
        }
        es?.close();
        es = null;
    }

    onMounted(() => {
        openStream();
    });

    onBeforeUnmount(() => {
        closeStream();
    });
}