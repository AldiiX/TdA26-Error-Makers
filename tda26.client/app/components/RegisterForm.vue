<script setup lang="ts">
import { ref, computed } from "vue";
import ButtonComponent from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import type {Account} from "#shared/types";

const emit = defineEmits<{
    (e: "registerSuccess", account: Account): void;
}>();

// state
const showPassword = ref(false);
const showPasswordApprove = ref(false);
const password = ref("");
const passwordApprove = ref("");
const isLoading = ref(false);
const errorMsg = ref<string[]>([]);
const email = ref("");

// handlers
function togglePassword() {
    showPassword.value = !showPassword.value;
}

function togglePasswordApprove() {
    showPasswordApprove.value = !showPasswordApprove.value;
}

function validaceEmailu(email: string): boolean {
    const val = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return val.test(email.toLowerCase());
}

const passwordRules = computed(() => ([
    { text: "Alespoň 8 znaků", pass: password.value.length >= 8 },
    { text: "Alespoň 1 malé písmeno", pass: /[a-z]/.test(password.value) },
    { text: "Alespoň 1 velké písmeno", pass: /[A-Z]/.test(password.value) },
    { text: "Alespoň 1 číslo", pass: /[0-9]/.test(password.value) },
    {
        text: "Alespoň 1 speciální znak",
        pass: /[!@#$%^&*()_\-+={}[\]:";'<>?,./\\|`~]/.test(password.value)
    },
    {
        text: "Hesla se musí shodovat",
        pass: password.value.length > 0 && password.value === passwordApprove.value
    }
]));

async function submitRegisterForm(event: Event) {
    const target = event.target as HTMLFormElement;
    const formData = new FormData(target);
    const formDataObj = Object.fromEntries(formData.entries());

    isLoading.value = true;
    errorMsg.value = [];

    if (!validaceEmailu(email.value)) {
        errorMsg.value.push("Neplatný formát e-mailu");
        isLoading.value = false;
        return;
    }
    
    for (const rule of passwordRules.value) {
        if (!rule.pass) errorMsg.value.push(rule.text);
    }

    if (errorMsg.value.length > 0) {
        isLoading.value = false;
        return;
    }

    try {
        const res = await $fetch<Account | null>("/api/v1/register", {
            method: "POST",
            body: formDataObj
        });

        if (!res) {
            throw new Error("Registration failed");
        }

        const loggedAccount = useState<Account | null>("loggedAccount", () => null);
        loggedAccount.value = res;

        emit("registerSuccess", res);
    } catch (err: any) {
        errorMsg.value = ["Uživatelské jméno nebo e-mail už existuje."];
    } finally {
        isLoading.value = false;
    }
}
</script>

<template>
    <form :class="$style.form" aria-describedby="form-error" :aria-busy="isLoading" @submit.prevent="submitRegisterForm">
        <div :class="$style.group">
            <label :class="$style.label" for="register-username">Uživatelské jméno</label>
            <Input
                id="register-username"
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
            <label :class="$style.label" for="register-email">E-mailová adresa</label>
            <Input
                id="register-email"
                v-model="email"
                style="width: 100%"
                name="email"
                type="text"
                inputmode="text"
                autocomplete="email"
                required
                placeholder="jannovak@seznam.cz"
            />
        </div>

        <div :class="$style.group">
            <label :class="$style.label" for="register-password">Heslo</label>
            <div :class="$style.passwordRow">
                <Input
                    id="register-password"
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
                    aria-controls="register-password"
                    aria-label="Zobrazit nebo skrýt heslo"
                    @click="togglePassword"
                >
                    {{ showPassword ? 'Skrýt' : 'Zobrazit' }}
                </button>
            </div>
        </div>
        
        <div :class="$style.group">
            <label :class="$style.label" for="register-password-approve">Potvrzení hesla</label>
            <div :class="$style.passwordRow">
                <Input
                    id="register-password-approve"
                    v-model="passwordApprove"
                    style="width: 100%"
                    placeholder="••••••••"
                    aria-describedby="password-help"
                    required
                    name="passwordApprove"
                    :type="showPasswordApprove ? 'text' : 'password'"
                />
                <button
                    :class="$style.toggle"
                    type="button"
                    :aria-pressed="showPasswordApprove"
                    aria-controls="register-password-approve"
                    aria-label="Zobrazit nebo skrýt heslo"
                    @click="togglePasswordApprove"
                >
                    {{ showPasswordApprove ? 'Skrýt' : 'Zobrazit' }}
                </button>
            </div>
        </div>

        <ButtonComponent button-style="primary" accent-color="primary" :class="$style.button" type="submit" :loading="isLoading">
            Zaregistrovat se
        </ButtonComponent>

        <div v-if="errorMsg.length > 0 || password.length > 0" :class="$style.errorGrid">
            <!-- Vetsi chyby (Mail, existujici uzivatel) -->
            <template v-if="errorMsg.length > 0">
                <p
                    v-for="(err, i) in errorMsg"
                    :key="'err-'+i"
                    :class="$style.errorItem"
                >
                    {{ err }}
                </p>
            </template>

            <!-- Password rules -->
            <template v-if="password.length > 0">
                <p
                    v-for="(rule, i) in passwordRules"
                    :key="'rule-'+i"
                    :class="[ $style.errorItem, rule.pass ? $style.ruleDone : '' ]"
                >
                    {{ rule.text }}
                </p>
            </template>
        </div>
    </form>
</template>

<style module lang="scss">
/* form */
.form {
    display: grid;
    gap: 14px;
}

.errorGrid {
    display: grid;
    gap: 6px; 
}

.errorItem {
    color: var(--accent-color-primary);
    margin: 0;
    font-size: 18px;
    padding-left: 14px;
    
    &::before {
        content: "•";
        display: inline-block;
        margin-right: 6px;
        font-weight: 900;
    }
}

.ruleDone {
    text-decoration: line-through;
    color: var(--accent-color-secondary-theme);
    opacity: 1 !important;
    transition: 0.25s ease;
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
