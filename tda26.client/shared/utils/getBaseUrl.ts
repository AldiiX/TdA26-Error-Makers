export default function(): string {
    /** client **/
    if (import.meta.client) { return window.location.origin; }

    /** server **/
    if (import.meta.prerender) { return  "http://localhost:3226"; }

    const isProd = !import.meta.dev;

    if (isProd) return "http://127.0.0.1";


    return "http://localhost:3226";
}