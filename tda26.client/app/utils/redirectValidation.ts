/**
 * Safely validates redirect URLs to prevent open redirect vulnerabilities
 * Only allows relative URLs within the same domain
 */
export function getSafeRedirectUrl(redirect: string | undefined, defaultUrl: string = '/'): string {
    if (!redirect || typeof redirect !== 'string') return defaultUrl;
    
    try {
        // Only validate on client-side where window is available
        if (typeof window !== 'undefined') {
            const currentOrigin = window.location.origin;
            const url = new URL(redirect, currentOrigin);
            
            // Only allow same origin URLs
            if (url.origin !== currentOrigin) {
                return defaultUrl;
            }
            
            // Return just the pathname + search + hash (no origin)
            return url.pathname + url.search + url.hash;
        }
    } catch {
        // If URL parsing fails, fall through to simple validation
    }
    
    // Server-side or parsing failed: use simple validation
    // Only accept if it starts with / and is not protocol-relative
    if (redirect.startsWith('/') && !redirect.startsWith('//')) {
        return redirect;
    }
    return defaultUrl;
}

/**
 * Gets a safe redirect URL for logout - avoids protected routes
 * For logout, we default to home instead of the current page
 */
export function getSafeLogoutRedirectUrl(currentPath: string): string {
    // List of paths that should redirect to home after logout
    const protectedPaths = ['/dashboard', '/admin'];
    
    // Check if current path starts with any protected path
    const isProtected = protectedPaths.some(path => currentPath.startsWith(path));
    
    // If on a protected route, redirect to home
    if (isProtected) {
        return '/';
    }
    
    // Otherwise stay on current page
    return getSafeRedirectUrl(currentPath, '/');
}
