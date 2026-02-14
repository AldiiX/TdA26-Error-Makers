import type { Ref } from "vue";
import { onMounted, onBeforeUnmount } from "vue";

export function useBeforeUnloadUnsavedChanges(isDirty: Ref<boolean>) {
    const handler = (e: BeforeUnloadEvent) => {
        if (!isDirty.value) return;

        e.preventDefault();
        e.returnValue = "";
    };

    onMounted(() => {
        // v dev modu tohle jen otravuje
        if (import.meta.dev) return;

        window.addEventListener("beforeunload", handler);
    });

    onBeforeUnmount(() => {
        if (import.meta.dev) return;

        window.removeEventListener("beforeunload", handler);
    });
}
