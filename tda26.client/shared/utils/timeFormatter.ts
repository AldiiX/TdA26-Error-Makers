export type DateFormatOptions = {
    locale?: string;
    withSeconds?: boolean;
    dateOnly?: boolean;
    timeOnly?: boolean;
};

export function formatDateTime(
    value: Date | string | number | null | undefined,
    options: DateFormatOptions = {}
): string | null {
    if (!value) return null;

    const {
        locale = "cs-CZ",
        withSeconds = false,
        dateOnly = false,
        timeOnly = false,
    } = options;

    const date = value instanceof Date ? value : new Date(value);

    if (isNaN(date.getTime())) return null;

    let formatOptions: Intl.DateTimeFormatOptions = {};

    if (dateOnly) {
        formatOptions = {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
        };
    } else if (timeOnly) {
        formatOptions = {
            hour: "2-digit",
            minute: "2-digit",
            ...(withSeconds && { second: "2-digit" }),
        };
    } else {
        formatOptions = {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
            ...(withSeconds && { second: "2-digit" }),
        };
    }

    return new Intl.DateTimeFormat(locale, formatOptions).format(date);
}