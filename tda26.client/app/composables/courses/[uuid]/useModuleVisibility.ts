import type { CourseModule, Material, Quiz } from "../../../../shared/types";
import type { Ref } from "vue";
import getBaseUrl from "#shared/utils/getBaseUrl";

export function useModuleVisibility(params: {
    courseUuid: string;
    module: Ref<CourseModule>;
}) {
    const isLoading = ref(false);
    const error = ref<string | null>(null);

    const isMaterial = (module: CourseModule): module is Material => {
        return 'type' in module && (module.type === 'file' || module.type === 'url');
    };

    const isQuiz = (module: CourseModule): module is Quiz => {
        return 'title' in module && 'questions' in module;
    };

    const changeVisibility = async (visible: boolean) => {
        error.value = null;
        isLoading.value = true;

        try {
            const module = params.module.value;

            if (isMaterial(module)) {
                // Update material visibility
                const updatedMaterial = await $fetch<Material>(getBaseUrl() + `/api/v1/courses/${params.courseUuid}/materials/${module.uuid}/visibility`, {
                    method: "PUT",
                    body: {
                        name: module.name,
                        description: module.description,
                        url: module.url,
                        isVisible: visible
                    }
                });
                
                params.module.value.isVisible = updatedMaterial.isVisible;
            } else if (isQuiz(module)) {
                // Update quiz visibility
                
                const updatedQuiz = await $fetch<Quiz>(getBaseUrl() + `/api/v1/courses/${params.courseUuid}/quizzes/${module.uuid}/visibility`, {
                    method: "PUT",
                    body: {
                        isVisible: visible,
                    }
                });
                
                params.module.value.isVisible = updatedQuiz.isVisible;
            }
        } catch (err) {
            console.error("Failed to update visibility:", err);
            error.value = "Nepodařilo se změnit viditelnost modulu.";
        } finally {
            isLoading.value = false;
        }
    };

    return {
        changeVisibility,
        isLoading,
        error
    };
}