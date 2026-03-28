import type { Account, DailyRewardDay, DailyRewardsMonthResponse } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";

type DailyRewardsApiError = {
    data?: {
        error?: string;
        message?: string;
    };
    message?: string;
};

const defaultTasks = [
    { taskCode: "view_material", title: "Zobrazit material", description: "Zobraz jeden material", targetValue: 1, rewardXp: 50, rewardDuck: 10 },
    { taskCode: "view_course", title: "Zobrazit kurz", description: "Otevri libovolny kurz", targetValue: 1, rewardXp: 35, rewardDuck: 8 },
    { taskCode: "submit_quiz", title: "Dokoncit kviz", description: "Odesli jeden kviz", targetValue: 1, rewardXp: 80, rewardDuck: 20 }
] as const;

export default function useDailyRewards() {
    const monthData = ref<DailyRewardsMonthResponse | null>(null);
    const loading = ref(false);
    const claiming = ref(false);
    const error = ref<string | null>(null);
    const loggedAccount = useState<Account | null>("loggedAccount", () => null);

    const syncWalletFromMonth = (month: DailyRewardsMonthResponse | null) => {
        if (!month || !loggedAccount.value) return;
        loggedAccount.value.dailyRewardXp = month.totalXp;
        loggedAccount.value.dailyRewardDucks = month.totalDucks;
    };

    const fetchMonth = async (year: number, month: number) => {
        loading.value = true;
        error.value = null;

        const headers = import.meta.server
            ? { Cookie: useRequestHeaders(["cookie"]).cookie || "" }
            : undefined;

        try {
            monthData.value = await $fetch<DailyRewardsMonthResponse>(`${getBaseUrl()}/api/v1/rewards/daily`, {
                query: { year, month },
                credentials: "include",
                headers
            });
            syncWalletFromMonth(monthData.value);
        } catch (e: unknown) {
            const apiError = e as DailyRewardsApiError;
            const serverMessage = apiError?.data?.error ?? apiError?.data?.message;

            // Fallback keeps UI usable when endpoint temporarily fails.
            monthData.value = createFallbackMonth(year, month);
            syncWalletFromMonth(monthData.value);
            error.value = serverMessage ?? "Nepodarilo se nacist odmeny. Zobrazuji zakladni rezim.";
        } finally {
            loading.value = false;
        }
    };

    const claimDay = async (date: string): Promise<DailyRewardDay | null> => {
        claiming.value = true;
        error.value = null;

        const headers = import.meta.server
            ? { Cookie: useRequestHeaders(["cookie"]).cookie || "" }
            : undefined;

        try {
            return await $fetch<DailyRewardDay>(`${getBaseUrl()}/api/v1/rewards/daily/${date}`, {
                method: "POST",
                credentials: "include",
                headers
            });
        } catch (e: unknown) {
            const apiError = e as DailyRewardsApiError;
            error.value = apiError?.data?.error ?? apiError?.data?.message ?? "Nepodarilo se odemknout denni odmenu.";
            return null;
        } finally {
            claiming.value = false;
        }
    };

    const createFallbackMonth = (year: number, month: number): DailyRewardsMonthResponse => {
        const daysInMonth = new Date(year, month, 0).getDate();
        const days: DailyRewardDay[] = [];

        for (let day = 1; day <= daysInMonth; day++) {
            const date = `${year}-${`${month}`.padStart(2, "0")}-${`${day}`.padStart(2, "0")}`;
            days.push({
                date,
                isClaimed: false,
                claimedAt: null,
                canClaim: false,
                isCompleted: false,
                tasks: defaultTasks.map((task) => ({
                    taskCode: task.taskCode,
                    title: task.title,
                    description: task.description,
                    currentValue: 0,
                    targetValue: task.targetValue,
                    isCompleted: false,
                    completedAt: null,
                    rewardXp: task.rewardXp,
                    rewardDuck: task.rewardDuck
                }))
            });
        }

        return {
            year,
            month,
            daysInMonth,
            totalXp: loggedAccount.value?.dailyRewardXp ?? 0,
            totalDucks: loggedAccount.value?.dailyRewardDucks ?? 0,
            days
        };
    };

    return {
        monthData,
        loading,
        claiming,
        error,
        fetchMonth,
        claimDay
    };
}


