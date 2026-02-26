import { ref, type Ref } from "vue";
import type { Course, Module } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import type { CourseDetailModal } from "~/composables/courses/[uuid]/courseDetailTypes";
import { push } from "notivue";

export interface ModuleFormModel {
    title: string;
    description: string;
}

function createEmptyModuleFormModel(): ModuleFormModel {
    return { title: "", description: "" };
}

function mapModuleToFormModel(module: Module): ModuleFormModel {
    return {
        title: module.title,
        description: module.description ?? "",
    };
}

export function useCourseModules(params: {
    course: Ref<Course | null>;
    enabledModal: Ref<CourseDetailModal>;
    isActionInProgress: Ref<boolean>;
    updateError: Ref<string | null>;
    deleteError: Ref<string | null>;
}) {
    const selectedModule = ref<Module | null>(null);
    const editingModule = ref<ModuleFormModel>(createEmptyModuleFormModel());

    const openCreateModuleModal = () => {
        selectedModule.value = null;
        editingModule.value = createEmptyModuleFormModel();
        params.enabledModal.value = "createModule";
    };

    const openUpdateModuleModal = (module: Module) => {
        selectedModule.value = module;
        editingModule.value = mapModuleToFormModel(module);
        params.enabledModal.value = "updateModule";
    };

    const openDeleteModuleModal = (module: Module) => {
        selectedModule.value = module;
        params.enabledModal.value = "deleteModule";
    };

    const handleModuleCreate = async () => {
        if (!params.course.value) return;

        params.updateError.value = null;

        const form = editingModule.value;
        if (!form.title.trim()) {
            params.updateError.value = "Název modulu je povinný.";
            return;
        }

        params.isActionInProgress.value = true;

        try {
            const maxOrder = Math.max(
                0,
                ...(params.course.value.modules ?? []).map(m => m.order)
            );
            const newModule = await $fetch<Module>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/modules`,
                {
                    method: "POST",
                    body: {
                        title: form.title.trim(),
                        description: form.description.trim() || undefined,
                        isVisible: false,
                        order: maxOrder + 1,
                    }
                }
            );

            params.course.value.modules = params.course.value.modules ?? [];
            params.course.value.modules.push(newModule);

            push.success({ title: "Modul vytvořen", message: "Modul byl úspěšně vytvořen.", duration: 4000 });
            params.enabledModal.value = null;
        } catch (err) {
            console.error("Module creation failed:", err);
            params.updateError.value = "Nepodařilo se vytvořit modul. Zkuste to prosím znovu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const handleModuleUpdate = async () => {
        if (!params.course.value || !selectedModule.value) return;

        params.updateError.value = null;

        const form = editingModule.value;
        if (!form.title.trim()) {
            params.updateError.value = "Název modulu je povinný.";
            return;
        }

        params.isActionInProgress.value = true;

        try {
            const updated = await $fetch<Module>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/modules/${selectedModule.value.uuid}`,
                {
                    method: "PUT",
                    body: {
                        title: form.title.trim(),
                        description: form.description.trim() || undefined,
                    }
                }
            );

            if (params.course.value.modules) {
                const idx = params.course.value.modules.findIndex(m => m.uuid === updated.uuid);
                if (idx !== -1) {
                    params.course.value.modules[idx] = {
                        ...params.course.value.modules[idx],
                        title: updated.title,
                        description: updated.description,
                        updatedAt: updated.updatedAt,
                    };
                }
            }

            push.success({ title: "Modul upraven", message: "Změny modulu byly uloženy.", duration: 4000 });
            params.enabledModal.value = null;
        } catch (err) {
            console.error("Module update failed:", err);
            params.updateError.value = "Nepodařilo se uložit změny modulu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const handleModuleDelete = async () => {
        if (!params.course.value || !selectedModule.value) return;

        params.deleteError.value = null;
        params.isActionInProgress.value = true;

        try {
            await $fetch<void>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/modules/${selectedModule.value.uuid}`,
                { method: "DELETE" }
            );

            params.course.value.modules = (params.course.value.modules ?? []).filter(
                m => m.uuid !== selectedModule.value!.uuid
            );

            push.success({ title: "Modul smazán", message: "Modul byl úspěšně smazán.", duration: 4000 });
            params.enabledModal.value = null;
        } catch (err) {
            console.error("Module delete failed:", err);
            params.deleteError.value = "Nepodařilo se smazat modul.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const toggleModuleVisibility = async (module: Module) => {
        if (!params.course.value) return;

        try {
            await $fetch<Module>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/modules/${module.uuid}`,
                {
                    method: "PUT",
                    body: { isVisible: !module.isVisible }
                }
            );

            if (params.course.value.modules) {
                const idx = params.course.value.modules.findIndex(m => m.uuid === module.uuid);
                if (idx !== -1) {
                    params.course.value.modules[idx].isVisible = !module.isVisible;
                }
            }
        } catch (err) {
            console.error("Module visibility toggle failed:", err);
        }
    };

    const reorderModules = async (orderedModules: Module[]) => {
        if (!params.course.value) return;

        const body = {
            modules: orderedModules.map((m, index) => ({ uuid: m.uuid, order: index })),
        };

        try {
            await $fetch(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/modules/order`,
                { method: "PUT", body }
            );
        } catch (err) {
            console.error("Module reorder failed:", err);
        }
    };

    return {
        selectedModule,
        editingModule,
        openCreateModuleModal,
        openUpdateModuleModal,
        openDeleteModuleModal,
        handleModuleCreate,
        handleModuleUpdate,
        handleModuleDelete,
        toggleModuleVisibility,
        reorderModules,
    };
}
