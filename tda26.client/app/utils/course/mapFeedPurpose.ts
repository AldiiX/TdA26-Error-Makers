import type { FeedPost, FeedPurposeType } from "#shared/types";

export function mapFeedPurpose(
    purpose?: FeedPost["purpose"],
    type?: FeedPost["type"]
): {
    label: string;
    type: FeedPurposeType;
    icon: string;
    color: string;
    background: string;
} {

    // manualni oznameni
    if (type === "manual") {
        return {
            label: "Oznámení",
            type: "announcement",
            icon: "/icons/megaphone.svg",
            color: "--accent-color-secondary-theme",
            background: "--accent-color-secondary-theme",
        };
    }

    switch (purpose) {

        // ===== material =====
        case "createMaterial":
            return {
                label: "Přidán materiál",
                type: "material",
                icon: "/icons/addFile.svg",
                color: "--accent-color-primary",
                background: "--accent-color-primary",
            };

        case "updateMaterial":
            return {
                label: "Upraven materiál",
                type: "material",
                icon: "/icons/editFile.svg",
                color: "--accent-color-primary",
                background: "--accent-color-primary",
            };

        case "deleteMaterial":
            return {
                label: "Smazán materiál",
                type: "material",
                icon: "/icons/deleteFile.svg",
                color: "--color-error",
                background: "--color-error",
            };

        case "showMaterial":
            return {
                label: "Zveřejněn materiál",
                type: "material",
                icon: "/icons/addFile.svg",
                color: "--accent-color-primary",
                background: "--accent-color-primary",
            };

        case "hideMaterial":
            return {
                label: "Skryt materiál",
                type: "material",
                icon: "/icons/deleteFile.svg",
                color: "--color-error",
                background: "--color-error",
            };

        // ===== quiz =====
        case "createQuiz":
            return {
                label: "Přidán kvíz",
                type: "quiz",
                icon: "/icons/addQuiz.svg",
                color: "--accent-color-primary",
                background: "--accent-color-primary",
            };

        case "updateQuiz":
            return {
                label: "Upraven kvíz",
                type: "quiz",
                icon: "/icons/editQuiz.svg",
                color: "--accent-color-primary",
                background: "--accent-color-primary",
            };

        case "deleteQuiz":
            return {
                label: "Smazán kvíz",
                type: "quiz",
                icon: "/icons/deleteQuiz.svg",
                color: "--color-error",
                background: "--color-error",
            };

        case "showQuiz":
            return {
                label: "Zveřejněn kvíz",
                type: "quiz",
                icon: "/icons/addQuiz.svg",
                color: "--accent-color-primary",
                background: "--accent-color-primary",
            };

        case "hideQuiz":
            return {
                label: "Skryt kvíz",
                type: "quiz",
                icon: "/icons/deleteQuiz.svg",
                color: "--color-error",
                background: "--color-error",
            };

        // ===== fallback =====
        default:
            return {
                label: "Aktivita",
                type: "announcement",
                icon: "/icons/activity.svg",
                color: "--accent-color-secondary-theme",
                background: "--accent-color-secondary-theme",
            };
    }
}
