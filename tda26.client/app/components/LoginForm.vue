<script setup lang="ts">
import { ref } from "vue";
import ButtonComponent from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import { normalizeAccountNumericFields, normalizeAccountType, type Account } from "#shared/types";
import { push } from "notivue";
import {useCourses} from "~/composables/useCourses";

const emit = defineEmits<{
    (e: "loginSuccess", account: Account): void;
}>();

// state
const showPassword = ref(false);
const password = ref("");
const isLoading = ref(false);
const errorMsg = ref<string | null>(null);
const { invalidateCoursesState } = useCourses();

// handlers
function togglePassword() {
    showPassword.value = !showPassword.value;
}

async function submitLoginForm(event: Event) {
    const target = event.target as HTMLFormElement;
    const formData = new FormData(target);
    const formDataObj = Object.fromEntries(formData.entries());

    const loginToast = push.promise({
        title: "Přihlášování",
        message: "Probíhá ověřování přihlašovacích údajů...",
        duration: Infinity
    });

    isLoading.value = true;
    errorMsg.value = null;

    try {
        const res = await $fetch<Account | null>("/api/v1/auth/login", {
            method: "POST",
            body: formDataObj
        });

        if (!res) {
            throw new Error("Invalid credentials");
        }

        const normalizedAccount: Account = normalizeAccountNumericFields({
            ...res,
            type: normalizeAccountType(res.type)
        });
        const loggedAccount = useState<Account | null>("loggedAccount", () => null);
        loggedAccount.value = normalizedAccount;

        loginToast.resolve({
            message: "Úspěšně přihlášeno!",
            duration: 1200
        });

        emit("loginSuccess", normalizedAccount);
        invalidateCoursesState();
    } catch (err: any) {
        errorMsg.value = "Nesprávné uživatelské jméno nebo heslo.";

        loginToast.reject({
            title: "Chyba přihlášení",
            message: "Zkontroluj uživatelské jméno a heslo a zkus to znovu.",
            duration: 6000
        });
    } finally {
        isLoading.value = false;
    }
}
</script>

<template>
    <form :class="$style.form" aria-describedby="form-error" :aria-busy="isLoading" @submit.prevent="submitLoginForm">
        <div :class="$style.group">
            <label :class="$style.label" for="login-username">Uživatelské jméno</label>
            <Input
                id="login-username"
                style="width: 100%"
                name="username"
                type="text"
                inputmode="text"
                autocomplete="username"
                required
                placeholder="jan.novak"
            />
        </div>

        <div :class="$style.group">
            <label :class="$style.label" for="login-password">Heslo</label>
            <div :class="$style.passwordRow">
                <Input
                    id="login-password"
                    v-model="password"
                    style="width: 100%"
                    placeholder="••••••••"
                    aria-describedby="password-help"
                    required
                    name="password"
                    :type="showPassword ? 'text' : 'password'"
                />

                <button
                    :class="$style.toggle"
                    type="button"
                    :aria-pressed="showPassword"
                    aria-controls="login-password"
                    aria-label="Zobrazit nebo skrýt heslo"
                    @click="togglePassword"
                >
                    {{ showPassword ? 'Skrýt' : 'Zobrazit' }}
                </button>
            </div>
        </div>

        <ButtonComponent button-style="primary" accent-color="primary" :class="$style.button" type="submit" :loading="isLoading">
            Přihlásit se
        </ButtonComponent>

        <p v-if="errorMsg" id="form-error" :class="$style.error" role="alert" aria-live="polite">{{ errorMsg }}</p>
    </form>
</template>

<style module lang="scss">
/* form */
.form {
    display: grid;
    gap: 14px;
}

.error {
    color: var(--accent-color-secondary-theme);
    width: fit-content;
    font-size: 14px;
    text-align: center;
    margin: 0 auto;

    @media (prefers-color-scheme: light) {
        color: #7a0b0b;
        background: #fdecec;
        border-color: #f6c1c1;
    }
}

.group {
    display: grid;
    gap: 8px;
}

.label {
    font-size: 14px;
    opacity: 0.9;
}

.passwordRow {
    position: relative;
}

/* toggle password button */
.toggle {
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
    height: 32px;
    padding: 0 10px;
    border: 1px solid var(--input-border-color);
    background: transparent;
    border-radius: 10px;
    color: inherit;
    font-size: 12px;
    cursor: pointer;
    transition: border-color 0.2s ease, background 0.2s ease, transform 0.1s ease;

    &:hover {
        border-color: var(--accent-color-secondary-theme);
    }
    &:active {
        transform: translateY(-50%) scale(0.97);
    }
}

/* submit button */
.button {
    height: 46px;
    border-radius: 12px;
    border: 0;
    color: white;
    font-weight: 600;
    letter-spacing: 0.02em;
    cursor: pointer;
    transition: filter 0.2s ease, transform 0.06s ease, box-shadow 0.2s ease;
}
</style>
