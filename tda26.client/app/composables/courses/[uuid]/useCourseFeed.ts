import { ref, computed, onMounted, onBeforeUnmount, type Ref } from "vue";
import type { Course, FeedPost, FeedPostView } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import { push } from "notivue";
import type { CourseDetailModal } from "~/composables/courses/[uuid]/courseDetailTypes";
import { mapFeedPurpose } from "~/utils/course/mapFeedPurpose";

type FeedFilter = "all" | "material" | "quiz";

export function useCourseFeed(params: {
    uuid: string;
    course: Ref<Course | null>;
    enabledModal: Ref<CourseDetailModal>;
    isActionInProgress: Ref<boolean>;
    feedPostError: Ref<string | null>;
}) {

    const selectedFeedFilter = ref<FeedFilter>("all");
    const selectedFeedPost = ref<FeedPost | null>(null);

    const editingFeedPost = ref<{
        message: string;
        type: FeedPost["type"];
    }>({
        message: "",
        type: "manual"
    });

    const { data: feedData, pending: feedPending, error: feedError } = useFetch<FeedPost[]>(() => getBaseUrl() + `/api/v1/courses/${params.uuid}/feed`, {
        server: false,
        key: `course-${params.uuid}-feed`,
        lazy: true,
        method: "GET",
    });

    const feedPosts = computed<FeedPostView[]>(() => {
        if (!feedData.value) return [];

        return feedData.value
            .filter(post => {
                switch (selectedFeedFilter.value) {
                    case "all":
                        return true;

                    case "material":
                        return post.purpose === "createMaterial" || post.purpose === "updateMaterial" || post.purpose === "deleteMaterial" || post.purpose === "showMaterial" || post.purpose === "hideMaterial";

                    case "quiz":
                        return post.purpose === "createQuiz" || post.purpose === "updateQuiz" || post.purpose === "deleteQuiz" || post.purpose === "showQuiz" || post.purpose === "hideQuiz";

                    default:
                        return true;
                }
            })
            .map(post => {
                const mapped = mapFeedPurpose(post.purpose, post.type);

                return {
                    ...post,
                    purposeLabel: mapped.label,
                    purposeType: mapped.type,
                    icon: mapped.icon,
                    color: mapped.color,
                    background: mapped.background ?? mapped.color
                } as FeedPostView;
            });
    });

    const openCreateFeedPost = () => {
        editingFeedPost.value.message = "";
        editingFeedPost.value.type = "manual";
        params.feedPostError.value = null;
        params.enabledModal.value = "createFeedPost";
    };

    const openUpdateFeedPost = (post: FeedPost) => {
        selectedFeedPost.value = post;

        editingFeedPost.value.message = post.message;
        editingFeedPost.value.type = post.type;

        params.feedPostError.value = null;
        params.enabledModal.value = "updateFeedPost";
    };

    const openDeleteFeedPost = (post: FeedPost) => {
        selectedFeedPost.value = post;
        params.feedPostError.value = null;
        params.enabledModal.value = "deleteFeedPost";
    };

    const handleFeedPostCreate = async () => {
        if (!params.course.value) return;

        if (!editingFeedPost.value.message.trim()) {
            params.feedPostError.value = "Text příspěvku nesmí být prázdný.";
            return;
        }

        params.isActionInProgress.value = true;
        params.feedPostError.value = null;

        try {
            await $fetch<FeedPost>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/feed`,
                {
                    method: "POST",
                    body: {
                        message: editingFeedPost.value.message,
                        type: editingFeedPost.value.type
                    }
                }
            );

            push.success({
                title: "Příspěvek přidán",
                message: "Příspěvek byl úspěšně publikován.",
                duration: 4000
            });

            params.enabledModal.value = null;
            editingFeedPost.value.message = "";
            editingFeedPost.value.type = "manual";

        } catch (err) {
            console.error(err);
            params.feedPostError.value = "Nepodařilo se vytvořit příspěvek.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const handleFeedPostUpdate = async () => {
        if (!params.course.value || !selectedFeedPost.value) return;

        if (!editingFeedPost.value.message.trim()) {
            params.feedPostError.value = "Text příspěvku nesmí být prázdný.";
            return;
        }

        params.isActionInProgress.value = true;
        params.feedPostError.value = null;

        try {
            const updated = await $fetch<FeedPost>(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/feed/${selectedFeedPost.value.uuid}`,
                {
                    method: "PUT",
                    body: {
                        message: editingFeedPost.value.message,
                        type: editingFeedPost.value.type
                    }
                }
            );

            feedData.value = feedData.value?.map(fp =>
                fp.uuid === updated.uuid ? updated : fp
            ) ?? [];

            push.success({
                title: "Příspěvek upraven",
                message: "Změny byly uloženy.",
                duration: 4000
            });

            params.enabledModal.value = null;
            selectedFeedPost.value = null;

            editingFeedPost.value.message = "";
            editingFeedPost.value.type = "manual";

        } catch (err) {
            console.error(err);
            params.feedPostError.value = "Nepodařilo se upravit příspěvek.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    const handleFeedPostDelete = async () => {
        if (!params.course.value || !selectedFeedPost.value) return;

        params.isActionInProgress.value = true;
        params.feedPostError.value = null;

        try {
            await $fetch(
                `${getBaseUrl()}/api/v1/courses/${params.course.value.uuid}/feed/${selectedFeedPost.value.uuid}`,
                { method: "DELETE" }
            );

            feedData.value = feedData.value?.filter(
                fp => fp.uuid !== selectedFeedPost.value?.uuid
            ) ?? [];

            push.success({
                title: "Příspěvek smazán",
                message: "Příspěvek byl odstraněn.",
                duration: 4000
            });

            params.enabledModal.value = null;
            selectedFeedPost.value = null;

        } catch (err) {
            console.error(err);
            params.feedPostError.value = "Nepodařilo se smazat příspěvek.";
        } finally {
            params.isActionInProgress.value = false;
        }
    };

    function onNewFeedPost(event: MessageEvent) {
        try {
            const post: FeedPost = JSON.parse(event.data);

            // ochrana proti duplicitam (refresh + sse)
            if (feedData.value?.some(p => p.uuid === post.uuid)) {
                return;
            }

            feedData.value = feedData.value
                ? [post, ...feedData.value]
                : [post];
        } catch (e) {
            console.error("Failed to parse feed SSE event", e);
        }
    }

    function onUpdateFeedPost(event: MessageEvent) {
        try {
            const post: FeedPost = JSON.parse(event.data);

            feedData.value = feedData.value?.map(fp =>
                fp.uuid === post.uuid ? post : fp
            ) ?? [];
        } catch (e) {
            console.error("Failed to parse update_post SSE event", e);
        }
    }

    function onDeleteFeedPost(event: MessageEvent) {
        try {
            const data: { uuid: string } = JSON.parse(event.data);

            feedData.value = feedData.value?.filter(
                fp => fp.uuid !== data.uuid
            ) ?? [];
        } catch (e) {
            console.error("Failed to parse delete_post SSE event", e);
        }
    }

    let feedEventSource: EventSource | null = null;

    onMounted(() => {
        if (!import.meta.client || !params.uuid) return;

        const url = `${getBaseUrl()}/api/v1/courses/${params.uuid}/feed/stream`;

        feedEventSource = new EventSource(url, {
            withCredentials: true
        });

        feedEventSource.addEventListener("new_post", onNewFeedPost);
        feedEventSource.addEventListener("update_post", onUpdateFeedPost);
        feedEventSource.addEventListener("delete_post", onDeleteFeedPost);

        feedEventSource.onerror = (err) => {
            console.error("SSE feed error", err);
        };
    });

    onBeforeUnmount(() => {
        if (feedEventSource) {
            feedEventSource.close();
            feedEventSource = null;
        }
    });

    return {
        selectedFeedFilter,
        feedData,
        feedPending,
        feedError,
        feedPosts,
        selectedFeedPost,
        editingFeedPost,
        openCreateFeedPost,
        openUpdateFeedPost,
        openDeleteFeedPost,
        handleFeedPostCreate,
        handleFeedPostUpdate,
        handleFeedPostDelete,
    };
}
