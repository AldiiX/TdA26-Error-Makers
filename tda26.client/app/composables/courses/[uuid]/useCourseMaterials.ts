import { ref, type Ref } from "vue";
import type { Course, Material } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import type { CourseDetailModal } from "~/composables/courses/[uuid]/courseDetailTypes";
import { formatUrl } from "~/utils/course/formatUrl";

export type MaterialFormModel =
    | {
    type: "url";
    name: string;
    description: string;
    url: string;
    file: null;
}
    | {
    type: "file";
    name: string;
    description: string;
    url: "";
    file: File | null;
};

function createEmptyMaterialFormModel(type: "file" | "url" = "file"): MaterialFormModel {
    if (type === "url") {
        return {
            type: "url",
            name: "",
            description: "",
            url: "",
            file: null,
        };
    }

    return {
        type: "file",
        name: "",
        description: "",
        url: "",
        file: null,
    };
}

function mapMaterialToFormModel(material: Material): MaterialFormModel {
    if (material.type === "url") {
        return {
            type: "url",
            name: material.name ?? "",
            description: material.description ?? "",
            url: material.url ?? "",
            file: null,
        };
    }

    return {
        type: "file",
        name: material.name ?? "",
        description: material.description ?? "",
        url: "",
        file: null,
    };
}

export function useCourseMaterials(params: {
    course: Ref<Course | null>;
    enabledModal: Ref<CourseDetailModal>;
    isActionInProgress: Ref<boolean>;
    updateError: Ref<string | null>;
    deleteError: Ref<string | null>;
}) {

    const selectedMaterial = ref<Material | null>(null);

    // nevyplnuj null, protoze v-model na MaterialFormItem chce vzdy validni model
    // a volar pak nebude rvat TS2322/TS18047
    const editingMaterial = ref<MaterialFormModel>(createEmptyMaterialFormModel("file"));

    const openCreateMaterialModal = () => {
        selectedMaterial.value = null;
        editingMaterial.value = createEmptyMaterialFormModel("file");
        params.enabledModal.value = "createMaterial";
    };

    const openUpdateMaterialModal = (material: Material) => {
        selectedMaterial.value = material;
        editingMaterial.value = mapMaterialToFormModel(material);
        params.enabledModal.value = "updateMaterial";
    };

    const openDeleteMaterialModal = (material: Material) => {
        selectedMaterial.value = material;
        params.enabledModal.value = "deleteMaterial";
    };

    const handleMaterialUpdate = async () => {
        if (!params.course.value || !selectedMaterial.value) return;

        params.updateError.value = null;

        const material = selectedMaterial.value;
        const edited = editingMaterial.value;

        if (edited.name.trim().length === 0) {
            params.updateError.value = "Název materiálu je povinný.";
            return;
        }

        params.isActionInProgress.value = true;

        const url = getBaseUrl() + `/api/v2/courses/${params.course.value.uuid}/materials/${material.uuid}`;

        try {
            let updatedMaterial: Material;

            // update podle original materialu (typ se realne v appce nemeni)
            if (material.type === "url") {
                if (edited.type !== "url") {
                    params.updateError.value = "Typ materiálu nelze změnit.";
                    return;
                }

                try {
                    edited.url = formatUrl(edited.url ?? "");
                } catch {
                    params.updateError.value = "Zadaná URL adresa není platná.";
                    return;
                }

                updatedMaterial = await $fetch<Material>(url, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: {
                        name: edited.name,
                        description: edited.description,
                        url: edited.url,
                    }
                });
            } else {
                if (edited.type !== "file") {
                    params.updateError.value = "Typ materiálu nelze změnit.";
                    return;
                }

                const form = new FormData();
                form.append("Name", edited.name ?? "");
                form.append("Description", edited.description ?? "");

                // pokud je vybran novy soubor, posli ho
                if (edited.file instanceof File) {
                    form.append("File", edited.file);
                }

                updatedMaterial = await $fetch<Material>(url, {
                    method: "PUT",
                    body: form
                });
            }

            // update local state
            params.course.value.materials = (params.course.value.materials ?? []).map(m =>
                m.uuid === updatedMaterial.uuid ? updatedMaterial : m
            );

            params.enabledModal.value = null;

        } catch (err) {
            console.error("Update failed:", err);
            params.updateError.value = "Nepodařilo se uložit změny materiálu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const handleMaterialDelete = async () => {
        if (!params.course.value || !params.course.value.materials) return;

        params.deleteError.value = null;
        params.isActionInProgress.value = true;

        try {
            await $fetch<void>(getBaseUrl() + `/api/v2/courses/${params.course.value.uuid}/materials/${selectedMaterial.value?.uuid}`, {
                method: "DELETE"
            });

            params.course.value.materials = params.course.value.materials.filter(m => m.uuid !== selectedMaterial.value?.uuid);

            params.enabledModal.value = null;
        } catch (err) {
            console.error("Error deleting material:", err);
            params.deleteError.value = "Nepodařilo se smazat materiál. Zkuste to prosím znovu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const handleMaterialCreate = async () => {
        if (!params.course.value) return;

        params.updateError.value = null;

        const edited = editingMaterial.value;

        if (edited.name.trim().length === 0) {
            params.updateError.value = "Název materiálu je povinný.";
            return;
        }

        const url = getBaseUrl() + `/api/v2/courses/${params.course.value.uuid}/materials`;

        params.isActionInProgress.value = true;

        try {
            let createdMaterial: Material;

            if (edited.type === "url") {
                try {
                    edited.url = formatUrl(edited.url ?? "");
                } catch {
                    params.updateError.value = "Zadaná URL adresa není platná.";
                    return;
                }

                createdMaterial = await $fetch<Material>(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: {
                        type: "url",
                        name: edited.name,
                        description: edited.description,
                        url: edited.url,
                    }
                });
            } else {
                if (!(edited.file instanceof File)) {
                    params.updateError.value = "Vyber soubor.";
                    return;
                }

                const form = new FormData();
                form.append("Type", "file");
                form.append("Name", edited.name ?? "");
                form.append("Description", edited.description ?? "");
                form.append("File", edited.file);

                createdMaterial = await $fetch<Material>(url, {
                    method: "POST",
                    body: form
                });
            }

            params.course.value.materials = params.course.value.materials ?? [];
            params.course.value.materials.unshift(createdMaterial);

            params.enabledModal.value = null;

        } catch (err) {
            console.error("Creation failed:", err);
            params.updateError.value = "Nepodařilo se vytvořit materiál. Zkuste to prosím znovu.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    return {
        selectedMaterial,
        editingMaterial,
        openCreateMaterialModal,
        openUpdateMaterialModal,
        openDeleteMaterialModal,
        handleMaterialUpdate,
        handleMaterialDelete,
        handleMaterialCreate,
    };
}