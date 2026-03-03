import { ref, type Ref } from "vue";
import type { Course, Quiz } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import { push } from "notivue";
import type { CourseDetailModal } from "~/composables/courses/[uuid]/courseDetailTypes";

export function useCourseQuizzes(params: {
    course: Ref<Course | null>;
    enabledModal: Ref<CourseDetailModal>;
    isActionInProgress: Ref<boolean>;
    updateError: Ref<string | null>;
    deleteError: Ref<string | null>;
    targetModuleUuid?: Ref<string | null>;
}) {

    const selectedQuiz = ref<Quiz | null>(null);

    const handleQuizDelete = async () => {
        if (!params.course.value) return;

        params.deleteError.value = null;
        params.isActionInProgress.value = true;

        const deletedUuid = selectedQuiz.value?.uuid;

        try {
            await $fetch<void>(getBaseUrl() + `/api/v1/courses/${params.course.value.uuid}/quizzes/${deletedUuid}`, {
                method: "DELETE"
            });

            // Remove from flat list
            if (params.course.value.quizzes) {
                params.course.value.quizzes = params.course.value.quizzes.filter(q => q.uuid !== deletedUuid);
            }

            // Remove from any module that contains it
            for (const mod of params.course.value.modules ?? []) {
                if (mod.quizzes) {
                    mod.quizzes = mod.quizzes.filter(q => q.uuid !== deletedUuid);
                }
            }

            push.success({
                title: "Kvíz smazán",
                message: "Kvíz byl úspěšně smazán.",
                duration: 4000
            });

            params.enabledModal.value = null;
        } catch (err) {
            console.error("Error deleting quiz:", err);
            params.deleteError.value = "Nepodařilo se smazat kvíz. Zkuste to prosím znovu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const handleQuizCreate = async (e: Event) => {
        if (!params.course.value) return;

        params.updateError.value = null;

        const form = e.target as HTMLFormElement;
        const formData = new FormData(form);
        const quizName = formData.get("createQuizName")?.toString().trim();

        if (!quizName) {
            params.updateError.value = "Název kvízu je povinný.";
            return;
        }

        params.isActionInProgress.value = true;

        try {
            const newQuiz = await $fetch<Quiz>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/quizzes`,
                {
                    method: "POST",
                    body: {
                        title: quizName,
                        moduleUuid: params.targetModuleUuid?.value ?? undefined,
                    }
                }
            );

            const moduleUuid = params.targetModuleUuid?.value;
            if (params.targetModuleUuid) params.targetModuleUuid.value = null;

            if (moduleUuid && params.course.value.modules) {
                const mod = params.course.value.modules.find(m => m.uuid === moduleUuid);
                if (mod) {
                    mod.quizzes = mod.quizzes ?? [];
                    newQuiz.createdAt = new Date().toISOString();
                    mod.quizzes.push(newQuiz);

                    push.success({
                        title: "Kvíz vytvořen",
                        message: "Kvíz byl úspěšně vytvořen.",
                        duration: 4000
                    });

                    params.enabledModal.value = null;
                    form.reset();
                    return;
                }
            }

            params.course.value.quizzes = params.course.value.quizzes ?? [];
            newQuiz.createdAt = new Date().toISOString();
            params.course.value.quizzes.unshift(newQuiz);

            push.success({
                title: "Kvíz vytvořen",
                message: "Kvíz byl úspěšně vytvořen.",
                duration: 4000
            });

            params.enabledModal.value = null;
            form.reset();
        } catch (err) {
            console.error("Creation failed:", err);
            params.updateError.value = "Nepodařilo se vytvořit kvíz. Zkuste to prosím znovu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    return {
        selectedQuiz,
        handleQuizCreate,
        handleQuizDelete,
    };
}
