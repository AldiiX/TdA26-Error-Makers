import type {Course, CourseStatus} from "#shared/types";
import { push } from "notivue";

export function useCourseStatus(params: {
    course: Ref<Course>;
}) {
    const statusOptions: CourseStatus[] = ["draft", "scheduled", "live", "paused", "archived"];
    const isLoading = ref(false);
    const error = ref<string | null>(null);

    const currentStatus = computed<CourseStatus>(() => params.course.value?.status);
    
    const updateCourseStatus = async (newStatus: CourseStatus) => {
        isLoading.value = true;
        error.value = null;

        try {
            const updated = await $fetch<Course>(
                `/api/v1/courses/${params.course.value.uuid}/status`,
                {
                    method: "PUT",
                    body: { status: newStatus },
                }
            );

            params.course.value.status = updated.status;
            push.success({ title: "Status kurzu změněn", message: `Status kurzu byl změněn na „${updated.status}".`, duration: 4000 });
        } catch (err: any) {
            error.value = err?.message ?? "Failed to update course status.";
            push.error({ title: "Chyba", message: "Nepodařilo se změnit status kurzu.", duration: 4000 });
        } finally {
            isLoading.value = false;
        }
    };
    
    return {
        statusOptions,
        currentStatus,
        isLoading,
        error,
        updateCourseStatus,
    };
}