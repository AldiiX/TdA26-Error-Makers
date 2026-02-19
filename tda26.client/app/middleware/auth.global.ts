import type {Account} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";

export default defineNuxtRouteMiddleware(async () => {
    const loggedAccount = useState<Account | null>('loggedAccount', () => null);

    // fetchnuti api
    if (loggedAccount.value === null) {
        let account: Account | null = null;

        try {
            account = await $fetch<Account>(getBaseUrl() + '/api/v1/me', {
                method: 'GET',
                headers: {
                    'Cookie': useRequestHeaders(['cookie']).cookie || ''
                }
            })
        }

        catch (e) {}

        loggedAccount.value = account;
        //console.log(loggedAccount.value)
    }
})