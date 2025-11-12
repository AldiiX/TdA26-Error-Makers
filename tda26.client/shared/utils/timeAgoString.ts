export default function (raw?: string | number | Date): string {
    if (!raw) return "";
    const date = new Date(raw);
    const diff = Date.now() - date.getTime();
    const s = Math.floor(diff / 1000);
    if (s < 60) return s === 1 ? "před 1 sekundou" : `před ${s} sekundami`;
    const m = Math.floor(s / 60);
    if (m < 60) return m === 1 ? "před 1 minutou" : `před ${m} minutami`;
    const h = Math.floor(m / 60);
    if (h < 24) return h === 1 ? "před 1 hodinou" : `před ${h} hodinami`;
    const d = Math.floor(h / 24);
    if (d < 30) return d === 1 ? "před 1 dnem" : `před ${d} dny`;
    const mo = Math.floor(d / 30);
    if (mo < 12) return mo === 1 ? "před 1 měsícem" : `před ${mo} měsíci`;
    const y = Math.floor(mo / 12);
    return y === 1 ? "před 1 rokem" : `před ${y} lety`;
};