import {useCourses} from "~/composables/courses/useCourses";
import type {Account} from "#shared/types";

export default function(){
    const { invalidateCoursesState } = useCourses();
    const loggedAccount = useState<Account | null>('loggedAccount', () => null);

    async function logout() {
        navigateTo('/');

        try {
            await $fetch('/api/v2/auth/logout', {
                method: 'POST'
            });
        } catch (err) {
            console.error('Logout error:', err);
        } finally {
            navigateTo('/');
            loggedAccount.value = null;
            invalidateCoursesState();
        }
    }

    return {
        loggedAccount,
        logout,
        //login,
    }
}