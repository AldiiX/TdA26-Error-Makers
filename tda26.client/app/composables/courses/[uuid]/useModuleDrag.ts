import type {Course, CourseModule} from "#shared/types";

type ModuleWithType = CourseModule & { moduleType: 'quiz' | 'material' };

export function useModuleDrag(params: {
    modules: Ref<ModuleWithType[]>;
    course: Ref<Course | null>;
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

    function onModuleDragLeave(event: DragEvent, moduleUuid: string) {
        // Only clear if we're truly leaving the li, not just entering a child element
        const relatedTarget = event.relatedTarget as Node | null;
        const currentTarget = event.currentTarget as HTMLElement | null;
        if (currentTarget && relatedTarget && currentTarget.contains(relatedTarget)) {
            return;
        }
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

        if (draggedIndex === -1) {
            draggedModuleUuid.value = null;
            return;
        }

        const [draggedItem] = currentModules.splice(draggedIndex, 1);
        if (!draggedItem) {
            draggedModuleUuid.value = null;
            return;
        }

        if (targetModuleUuid === '__end__') {
            currentModules.push(draggedItem);
        } else {
            const targetIndex = currentModules.findIndex(m => m.uuid === targetModuleUuid);
            if (targetIndex === -1) {
                draggedModuleUuid.value = null;
                return;
            }
            currentModules.splice(targetIndex, 0, draggedItem);
        }

        // Build new order map: uuid -> new order index
        const newOrderMap = new Map<string, number>();
        currentModules.forEach((m, index) => {
            newOrderMap.set(m.uuid, index);
        });

        // Update order on the source course materials & quizzes so the computed re-sorts
        const courseVal = params.course.value;
        if (courseVal) {
            if (courseVal.materials) {
                for (const mat of courseVal.materials) {
                    const newOrd = newOrderMap.get(mat.uuid);
                    if (newOrd !== undefined) {
                        mat.order = newOrd;
                    }
                }
            }
            if (courseVal.quizzes) {
                for (const quiz of courseVal.quizzes) {
                    const newOrd = newOrderMap.get(quiz.uuid);
                    if (newOrd !== undefined) {
                        quiz.order = newOrd;
                    }
                }
            }
            // Trigger reactivity by reassigning
            params.course.value = { ...courseVal };
        }

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