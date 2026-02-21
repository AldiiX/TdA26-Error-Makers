import type { Ref } from "vue";
import type { Course } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import { push } from "notivue";
import type { CourseDetailModal } from "~/composables/courses/[uuid]/courseDetailTypes";

export function useCourseDelete(params: {
    courseSmall: Ref<Course>;
    enabledModal: Ref<CourseDetailModal>;
    isActionInProgress: Ref<boolean>;
    deleteError: Ref<string | null>;
    clearCourseCaches: () => void;
}) {

    const openDeleteCourseModal = () => {
        params.deleteError.value = null;
        params.enabledModal.value = "deleteCourse";
    };

    const handleCourseDelete = async () => {
        if (!params.courseSmall.value) return;

        params.isActionInProgress.value = true;
        params.deleteError.value = null;

        try {
            await $fetch<void>(getBaseUrl() + `/api/v1/courses/${params.courseSmall.value.uuid}`, {
                method: "DELETE"
            });

            params.clearCourseCaches();

            push.success({
                title: "Kurz smazán",
                message: "Kurz byl úspěšně smazán.",
                duration: 4000
            });

            await navigateTo("/courses");
        } catch (err) {
            console.error("Error deleting course:", err);
            params.deleteError.value = "Nepodařilo se smazat kurz. Zkuste to prosím znovu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    return {
        openDeleteCourseModal,
        handleCourseDelete,
    };
}
