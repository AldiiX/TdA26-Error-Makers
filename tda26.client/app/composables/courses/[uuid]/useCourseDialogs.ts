import { ref, watch } from "vue";
import type { CourseDetailModal } from "~/composables/courses/[uuid]/courseDetailTypes";


export function useCourseDialogs() {

    const enabledModal =  useState <CourseDetailModal>("useCoursesDialogs__enabledModal", () => null);
    
    const authTab = ref<"login" | "register">("login");

    const updateError = ref<string | null>(null);
    const deleteError = ref<string | null>(null);
    const feedPostError = ref<string | null>(null);

    watch(enabledModal, (val) => {
        if (val === null) return;

        // pri otevreni modalu resetni errory
        updateError.value = null;
        deleteError.value = null;
        feedPostError.value = null;
    });

    return {
        enabledModal,
        authTab,
        updateError,
        deleteError,
        feedPostError,
    };
}
