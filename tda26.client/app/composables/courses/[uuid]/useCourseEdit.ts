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

    const editedStatus = ref<CourseStatus>(params.courseSmall.value.status);

    const displayedStatus = computed<CourseStatus>(() => params.course.value?.status ?? params.courseSmall.value.status);

    const editedStatusModel = computed<string>({
        get: () => String(editedStatus.value),
        set: (value) => {
            editedStatus.value = value as CourseStatus;
        }
    });

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

    watch(
        params.course,
        (val) => {
            if (!val) return;
            editedStatus.value = val.status;
        },
        { immediate: true }
    );

    const isDirty = computed(() => {
        if (!params.course.value || !params.originalCourse.value) return false;

        const originalTags = normalizeTags(
            params.originalCourse.value.tags?.map(t => t.uuid)
        );

        const editedTags = normalizeTags(editedTagsUuid.value);

        return (
            params.course.value.name !== params.originalCourse.value.name ||
            params.course.value.description !== params.originalCourse.value.description ||
            editedStatus.value !== params.originalCourse.value.status ||
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

        try {
            const updatedCourse = await $fetch<Course>(
                getBaseUrl() + `/api/v2/courses/${params.course.value.uuid}`,
                {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: {
                        name: params.course.value.name,
                        description: params.course.value.description,
                        status: editedStatus.value,
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
            editedStatus.value = updatedCourse.status;

            clearCourseCaches();

            push.success({
                title: "Změny uloženy",
                message: "Změny kurzu byly úspěšně uloženy.",
                duration: 4000
            });

        } catch (err) {
            console.error(err);

            push.error({
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
        editedStatus,
        editedStatusModel,
        displayedStatus,
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
