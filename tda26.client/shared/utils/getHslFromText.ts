export default function(text: string): string {
    let hash = 0;
    for (let i = 0; i < text.length; i++) {
        hash = text.charCodeAt(i) + ((hash << 5) - hash);
    }
    hash = Math.abs(hash);
    const hue = hash % 360;
    const saturation = Math.floor((hash ?? 0) % 20 + 45);
    const lightness = Math.floor((hash ?? 0) % 20 + 45);
    return `hsl(${hue}, ${saturation}%, ${lightness}%)`;
}