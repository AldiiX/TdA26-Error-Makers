export default function toClockTime(timeInSeconds: number): string {
    timeInSeconds = Math.round(timeInSeconds);
    const hours = Math.floor(timeInSeconds / 3600);
    const minutes = Math.floor((timeInSeconds % 3600) / 60);
    const seconds = timeInSeconds % 60;

    const hoursStr = hours > 0 ? `${hours}:` : "";
    const minutesStr = hours > 0 ? String(minutes).padStart(2, '0') : String(minutes);
    const secondsStr = String(seconds).padStart(2, '0');

    return `${hoursStr}${minutesStr}:${secondsStr}`;
}