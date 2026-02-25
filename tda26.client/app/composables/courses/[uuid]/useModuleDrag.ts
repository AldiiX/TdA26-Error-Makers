import type {CourseModule} from "../../../../shared/types";

type ModuleWithType = CourseModule & { moduleType: 'quiz' | 'material' };

export function useModuleDrag(params: {
    modules: Ref<ModuleWithType[]>;
}) {
    const draggedModuleUuid = ref<string | null>(null);
    const dragOverModuleUuid = ref<string | null>(null);

    function onModuleDragStart(event: DragEvent, moduleUuid: string) {
        draggedModuleUuid.value = moduleUuid;
        if (event.dataTransfer) {
            event.dataTransfer.effectAllowed = 'move';
        }
    }

    function onModuleDragOver(event: DragEvent, moduleUuid: string) {
        event.preventDefault();
        if (event.dataTransfer) {
            event.dataTransfer.dropEffect = 'move';
        }
        dragOverModuleUuid.value = moduleUuid;
    }

    function onModuleDragLeave(_event: DragEvent, moduleUuid: string) {
        if (dragOverModuleUuid.value === moduleUuid) {
            dragOverModuleUuid.value = null;
        }
    }

    function onModuleDrop(event: DragEvent, targetModuleUuid: string) {
        event.preventDefault();
        dragOverModuleUuid.value = null;

        if (!draggedModuleUuid.value || draggedModuleUuid.value === targetModuleUuid) {
            draggedModuleUuid.value = null;
            return;
        }

        const currentModules = [...params.modules.value];
        const draggedIndex = currentModules.findIndex(m => m.uuid === draggedModuleUuid.value);
        const targetIndex = currentModules.findIndex(m => m.uuid === targetModuleUuid);

        if (draggedIndex === -1 || targetIndex === -1) {
            draggedModuleUuid.value = null;
            return;
        }

        const [draggedItem] = currentModules.splice(draggedIndex, 1);
        if (!draggedItem) {
            draggedModuleUuid.value = null;
            return;
        }
        currentModules.splice(targetIndex, 0, draggedItem);

        const newOrder = currentModules.map((m, index) => ({
            uuid: m.uuid,
            moduleType: m.moduleType,
            order: index,
        }));
        console.log('New module order:', newOrder);

        draggedModuleUuid.value = null;
    }

    function onModuleDragEnd() {
        draggedModuleUuid.value = null;
        dragOverModuleUuid.value = null;
    }

    return {
        draggedModuleUuid,
        dragOverModuleUuid,
        onModuleDragStart,
        onModuleDragOver,
        onModuleDragLeave,
        onModuleDrop,
        onModuleDragEnd,
    }
}