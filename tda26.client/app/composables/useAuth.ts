import {useCourses} from "~/composables/useCourses";
import type {Account} from "#shared/types";
import {getSafeLogoutRedirectUrl} from "~/utils/redirectValidation";

export default function(){
    const { invalidateCoursesState } = useCourses();
    const loggedAccount = useState<Account | null>('loggedAccount', () => null);
    const route = useRoute();

    async function logout() {
        // Capture current path for post-logout redirect
        const currentPath = route.fullPath;

        try {
            await $fetch('/api/v2/auth/logout', {
                method: 'POST'
            });
        } catch (err) {
            console.error('Logout error:', err);
        } finally {
            loggedAccount.value = null;
            invalidateCoursesState();
            // Redirect to safe URL (avoids protected routes)
            const redirectTo = getSafeLogoutRedirectUrl(currentPath);
            navigateTo(redirectTo);
        }
    }

    return {
        loggedAccount,
        logout,
        //login,
    }
}