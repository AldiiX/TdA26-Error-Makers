import formatCzechCount from "#shared/utils/formatCzechCount";

export default function timeToString(input: Date | string | number): string {
    const target = new Date(input).getTime();
    const now = Date.now();
    console.log(`target: ${target}, now: ${now}, diff: ${target - now}`);

    let diffMs = target - now;
    if (diffMs <= 0) {
        return "právě teď";
    }

    const seconds = Math.floor(diffMs / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);

    if (seconds < 60) {
        return `za ${seconds} ${formatCzechCount(seconds, {one: "sekundu", few: "sekundy", many: "sekund"})}`;
    }

    if (minutes < 60) {
        return `za ${minutes} ${formatCzechCount(minutes, {one: "minutu", few: "minuty", many: "minut"})}`;
    }

    if (hours < 24) {
        return `za ${hours} ${formatCzechCount(hours, {one: "hodinu", few: "hodiny", many: "hodin"})}`;
    }

    return `za ${days} ${formatCzechCount(days, {one: "den", few: "dny", many: "dní"})}`;
}