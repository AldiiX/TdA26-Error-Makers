import type {Course, CourseModule, Module} from "#shared/types";

export const DRAG_ITEM_KEY = 'application/flat-item';

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
        // Reject drops from inside a module section (application/module-item or application/flat-item
        // with a sourceModuleUuid) — those should only be accepted by actual module drop zones, not the unassigned list.
        if (event.dataTransfer?.types.includes('application/module-item')) return;
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

    async function onModuleDrop(event: DragEvent, targetModuleUuid: string) {
        // Ignore drops from module items — only handle unassigned-list reorders here.
        if (event.dataTransfer?.types.includes('application/module-item')) return;
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

        // console.log('New module order:', newOrder);

        draggedModuleUuid.value = null;

        await $fetch(`/api/v1/courses/${courseVal?.uuid}/module-order`, {
            method: 'POST',
            body: {
                moduleOrders: newOrder,
            },
        }).catch(err => {
            console.error('Error saving module order:', err);
        });
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
    };
}

// Drag for Module sections (grouping containers)
export function useModuleSectionDrag(params: {
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
        // Ignore drags that originate from inside a module (items being reordered or moved
        // between modules) — those must only land on CourseModuleSection drop zones.
        if (event.dataTransfer?.types.includes('application/module-item')) return;
        if (event.dataTransfer?.types.includes('application/flat-item')) return;
        event.preventDefault();
        if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
        dragOverModuleUuid.value = moduleUuid;
    }

    function onModuleDragLeave(event: DragEvent, moduleUuid: string) {
        const relatedTarget = event.relatedTarget as Node | null;
        const currentTarget = event.currentTarget as HTMLElement | null;
        if (currentTarget && relatedTarget && currentTarget.contains(relatedTarget)) return;
        if (dragOverModuleUuid.value === moduleUuid) dragOverModuleUuid.value = null;
    }

    async function onModuleDrop(event: DragEvent, targetUuid: string) {
        // Ignore drops from module items — only handle section-level reorders here.
        if (event.dataTransfer?.types.includes('application/module-item')) return;
        if (event.dataTransfer?.types.includes('application/flat-item')) return;
        event.preventDefault();
        dragOverModuleUuid.value = null;

        const courseVal = params.course.value;
        if (!courseVal?.modules || !draggedModuleUuid.value || draggedModuleUuid.value === targetUuid) {
            draggedModuleUuid.value = null;
            return;
        }

        const mods = [...courseVal.modules].sort((a, b) => a.order - b.order);
        const draggedIdx = mods.findIndex(m => m.uuid === draggedModuleUuid.value);
        if (draggedIdx === -1) { draggedModuleUuid.value = null; return; }

        const [dragged] = mods.splice(draggedIdx, 1);
        if (!dragged) { draggedModuleUuid.value = null; return; }

        if (targetUuid === '__end__') {
            mods.push(dragged);
        } else {
            const targetIdx = mods.findIndex(m => m.uuid === targetUuid);
            if (targetIdx === -1) { draggedModuleUuid.value = null; return; }
            mods.splice(targetIdx, 0, dragged);
        }

        // Optimistically update order values
        mods.forEach((m, i) => { m.order = i; });
        courseVal.modules = mods;
        params.course.value = { ...courseVal };

        draggedModuleUuid.value = null;

        const body = { modules: mods.map((m, i) => ({ uuid: m.uuid, order: i })) };
        await $fetch(`/api/v1/courses/${courseVal.uuid}/modules/order`, {
            method: 'PUT',
            body,
        }).catch(err => console.error('Error saving module section order:', err));
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
    };
}