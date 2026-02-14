export function formatUrl(url: string): string {
    let normalized = url.trim();

    if (!normalized.startsWith("http://") && !normalized.startsWith("https://")) {
        normalized = "https://" + normalized;
    }

    // jednoducha kontrola domeny + cesty (nechci tady mega validator)
    const urlRegex = new RegExp(/[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)/gi);

    if (!urlRegex.test(normalized)) {
        throw new Error("invalid url format");
    }

    const parsedUrl = new URL(normalized);

    return parsedUrl.href;
}
