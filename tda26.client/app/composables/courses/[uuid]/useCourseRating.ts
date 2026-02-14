import { ref, computed, watch, type Ref } from "vue";
import type { Account, Course } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import type { CourseDetailModal } from "~/composables/course/courseDetailTypes";

export function useCourseRating(params: {
    uuid: string;
    loggedAccount: Ref<Account | null>;
    courseSmall: Ref<Course>;
    course: Ref<Course | null>;
    enabledModal: Ref<CourseDetailModal>;
}) {

    const ratingLoading = ref(false);

    const isThisCourseLiked = computed(() => {
        if (!params.loggedAccount.value || !params.courseSmall.value) return false;
        return params.loggedAccount.value.likes.some(l => l.course?.uuid === params.courseSmall.value.uuid);
    });

    const isThisCourseDisliked = computed(() => {
        if (!params.loggedAccount.value || !params.courseSmall.value) return false;
        return params.loggedAccount.value.dislikes.some(l => l.course?.uuid === params.courseSmall.value.uuid);
    });

    const isThisCourseLikedDesign = ref<boolean>(isThisCourseLiked.value);
    const isThisCourseDislikedDesign = ref<boolean>(isThisCourseDisliked.value);
    const optimisticLikeCount = ref<number>(params.courseSmall.value?.likeCount ?? 0);

    // keep count in sync with fetched data
    watch(() => params.courseSmall.value?.likeCount, (newCount) => {
        if (newCount !== undefined) {
            optimisticLikeCount.value = newCount;
        }
    }, { immediate: true });

    async function addRating(rating: "like" | "dislike" | null) {
        // pokud neni prihlasen, otevri auth modal
        if (!params.loggedAccount.value) {
            params.enabledModal.value = "loginRequired";
            return;
        }

        if (!params.courseSmall.value || ratingLoading.value) return;

        const baseUrl = getBaseUrl();
        const url = baseUrl + `/api/v2/courses/${params.courseSmall.value.uuid}/rating`;

        // uloz predchozi stav pro rollback
        const previousLikedDesign = isThisCourseLikedDesign.value;
        const previousDislikedDesign = isThisCourseDislikedDesign.value;
        const previousLikeCount = optimisticLikeCount.value;

        switch (rating) {
            case "like": {
                if (isThisCourseLikedDesign.value) {
                    isThisCourseLikedDesign.value = false;
                    optimisticLikeCount.value--;
                    rating = null;
                } else {
                    isThisCourseLikedDesign.value = true;
                    isThisCourseDislikedDesign.value = false;
                    optimisticLikeCount.value++;
                }
            } break;

            case "dislike": {
                if (isThisCourseDislikedDesign.value) {
                    isThisCourseDislikedDesign.value = false;
                    rating = null;
                } else {
                    if (isThisCourseLikedDesign.value) {
                        optimisticLikeCount.value--;
                    }
                    isThisCourseDislikedDesign.value = true;
                    isThisCourseLikedDesign.value = false;
                }
            } break;
        }

        try {
            ratingLoading.value = true;

            await $fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: { type: rating }
            });

            // refresh vsech dat paralelne
            const [updatedUser, updatedCourseSmall, updatedCourse] = await Promise.all([
                $fetch<Account>(baseUrl + `/api/v2/me`, { method: "GET", headers: { "Content-Type": "application/json" } }),
                $fetch<Course>(baseUrl + `/api/v2/courses/${params.courseSmall.value.uuid}`, { method: "GET", headers: { "Content-Type": "application/json" }, query: { full: false } }),
                $fetch<Course>(baseUrl + `/api/v2/courses/${params.courseSmall.value.uuid}`, { method: "GET", headers: { "Content-Type": "application/json" }, query: { full: true } }),
            ]);

            params.loggedAccount.value = updatedUser ?? null;
            params.courseSmall.value = updatedCourseSmall;
            params.course.value = updatedCourse;

        } catch (err) {
            console.error("Error updating rating:", err);

            // rollback optimistic state
            isThisCourseLikedDesign.value = previousLikedDesign;
            isThisCourseDislikedDesign.value = previousDislikedDesign;
            optimisticLikeCount.value = previousLikeCount;

        } finally {
            ratingLoading.value = false;
        }
    }

    return {
        ratingLoading,
        isThisCourseLikedDesign,
        isThisCourseDislikedDesign,
        optimisticLikeCount,
        addRating,
    };
}
