export type DbStatus = 0 | 1 | 2 | 3 | 4;

const STATUS_TEXT_BY_CODE: Record<DbStatus, string> = {
    0: "Návrh",
    1: "Naplánováno",
    2: "Probíhá",
    3: "Pauza",
    4: "Archivováno",
};

export function statusToText(status: DbStatus): string {
    return STATUS_TEXT_BY_CODE[status];
}

export function textToStatus(text: string): DbStatus | null {
    const normalized = text.trim().toLowerCase();

    for (const [status, label] of Object.entries(STATUS_TEXT_BY_CODE)) {
        if (label.toLowerCase() === normalized) {
            return Number(status) as DbStatus;
        }
    }

    return null;
}
