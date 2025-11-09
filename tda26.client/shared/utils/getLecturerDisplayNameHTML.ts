import type { Lecturer } from "#shared/types";

export default function(lecturer: Lecturer) {
    const tb = lecturer.titleBefore ?? "";
    const ta = lecturer.titleAfter ?? "";
    const first = lecturer.firstName ?? "";
    const middle = lecturer.middleName ?? "";
    const last = lecturer.lastName ?? "";

    const name = [first, middle, last].filter(Boolean).join(" ");
    const before = tb ? tb + "\u00A0" : "";
    const after = ta ? ", " + ta : "";
    return `<span>${before}</span>${name}<span>${after}</span>`.trim();
}