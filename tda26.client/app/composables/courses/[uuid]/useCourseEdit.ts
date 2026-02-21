import { computed, ref, watch, type Ref } from "vue";
import type {Account, Course, CourseCategory, CourseStatus} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import { push } from "notivue";

const normalizeTags = (tags?: string[]) =>
    [...(tags ?? [])].sort();

export function useCourseEdit(params: {
    uuid: string;
    isEditMode: boolean;
    courseSmall: Ref<Course>;
    course: Ref<Course | null>;
    originalCourse: Ref<Course | null>;
    courseData: Ref<Course | null | undefined>;
    categories: Ref<CourseCategory[] | null | undefined>;
    loggedAccount: Ref<Account | null>;
    isActionInProgress: Ref<boolean>;
}) {

    const statusOptions: CourseStatus[] = ["draft", "scheduled", "live", "paused", "archived"];

    const editedCategoryUuid = ref<string | null>(
        params.courseSmall.value.category?.uuid ?? params.categories.value?.[0]?.uuid ?? null
    );

    const editedTagsUuid = ref<string[]>([]);

    watch(
        editedCategoryUuid,
        (newVal, oldVal) => {
            if (!oldVal) return;
            if (newVal === oldVal) return;

            // po zmene kategorie vymaz tagy
            editedTagsUuid.value = [];
        }
    );

    watch(params.courseData, (val) => {
        if (!val) return;

        // deep clone pro safe edit
        params.course.value = structuredClone(val);
        params.originalCourse.value = structuredClone(val);

        if (val.category?.uuid) editedCategoryUuid.value = val.category.uuid;
        editedTagsUuid.value = val.tags?.map(t => t.uuid) ?? [];
    }, { immediate: true });

    const isDirty = computed(() => {
        if (!params.course.value || !params.originalCourse.value) return false;

        const originalTags = normalizeTags(
            params.originalCourse.value.tags?.map(t => t.uuid)
        );

        const editedTags = normalizeTags(editedTagsUuid.value);

        return (
            params.course.value.name !== params.originalCourse.value.name ||
            params.course.value.description !== params.originalCourse.value.description ||
            editedCategoryUuid.value !== params.originalCourse.value.category?.uuid ||
            JSON.stringify(editedTags) !== JSON.stringify(originalTags)
        );
    });

    const ownsCourse = computed(() => {
        if (!params.loggedAccount.value || !params.courseSmall.value) return false;
        if (params.loggedAccount.value.type === "admin") return true;
        return params.loggedAccount.value.uuid === params.courseSmall.value?.account?.uuid;
    });

    const updateCourseTitle = (newTitle: string) => {
        if (!params.isEditMode) return;
        if (params.course.value) {
            params.course.value.name = newTitle;
        }
    };

    const updateCourseDescription = (newDescription: string) => {
        if (!params.isEditMode) return;
        if (params.course.value) {
            params.course.value.description = newDescription;
        }
    };

    const clearCourseCaches = () => {
        // usefetch cache
        clearNuxtData([
            `course-${params.uuid}-small`,
            `course-${params.uuid}-full`,
            `course-${params.uuid}-feed`,
        ]);

        // usestate cache + flagy
        clearNuxtState([
            "allCourses",
            "hasFetchedAllCourses",
            "myCoursesCache",
            "hasFetchedAllMyCourses",
        ]);
    };

    const saveCourseChanges = async () => {
        if (!params.course.value) return;

        params.isActionInProgress.value = true;

        const toast = push.promise({
            title: "Ukládám změny...",
            message: "Probíhá ukládání změn kurzu.",
            duration: Infinity
        });

        try {
            const updatedCourse = await $fetch<Course>(
                getBaseUrl() + `/api/v1/courses/${params.course.value.uuid}`,
                {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: {
                        name: params.course.value.name,
                        description: params.course.value.description,
                        categoryUuid: editedCategoryUuid.value,
                        tagsUuid: editedTagsUuid.value
                    }
                }
            );

            params.course.value = structuredClone(updatedCourse);
            params.originalCourse.value = structuredClone(updatedCourse);

            // sync maly kurz pro header
            params.courseSmall.value = {
                ...params.courseSmall.value,
                name: updatedCourse.name,
                description: updatedCourse.description,
                status: updatedCourse.status,
                category: updatedCourse.category,
                tags: updatedCourse.tags,
            } as Course;

            editedCategoryUuid.value = updatedCourse.category?.uuid ?? editedCategoryUuid.value;
            editedTagsUuid.value = updatedCourse.tags?.map(t => t.uuid) ?? [];

            clearCourseCaches();

            toast.resolve({
                title: "Změny uloženy",
                message: "Změny kurzu byly úspěšně uloženy.",
                duration: 4000
            });
        } catch (err) {
            console.error(err);

            toast.reject({
                title: "Chyba při ukládání",
                message: "Nepodařilo se uložit změny kurzu.",
                duration: 4000
            });
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    return {
        statusOptions,
        editedCategoryUuid,
        editedTagsUuid,
        isDirty,
        ownsCourse,
        saveCourseChanges,
        clearCourseCaches,
        updateCourseTitle,
        updateCourseDescription,
    };
}
