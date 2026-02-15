import type {CourseStatus} from "#shared/types";

const STATUS_TEXT_BY_CODE: Record<CourseStatus, string> = {
    "draft": "Návrh",
    "scheduled": "Naplánováno",
    "live": "Probíhá",
    "paused": "Pauza",
    "archived": "Archivováno",
};

export function statusToText(status: CourseStatus): string {
    return STATUS_TEXT_BY_CODE[status];
}

export function textToStatus(text: string): CourseStatus | null {
    const normalized = text.trim().toLowerCase();

    for (const [status, label] of Object.entries(STATUS_TEXT_BY_CODE)) {
        if (label.toLowerCase() === normalized) {
            return status as CourseStatus;
        }
    }

    return null;
}
