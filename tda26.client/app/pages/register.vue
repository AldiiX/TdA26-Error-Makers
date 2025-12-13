<script setup lang="ts">
import { ref } from "vue";
import { Head, Title } from "#components";
import BlurBackground from "~/components/backgrounds/BlurBackground.vue";
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
import ButtonComponent from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import type {Account} from "#shared/types";

definePageMeta({
    layout: "normal-page-layout"
});

// const loggedAccount = useState<Account | null>("loggedAccount", () => null);
// if (loggedAccount.value) {
//     navigateTo("/");
// }

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
    // prepina zobrazeni hesla
    showPassword.value = !showPassword.value;
}
function togglePasswordApprove() {
    // prepina zobrazeni podtvrzeni hesla
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
        const res = await $fetch<Account | null>("/api/v2/register", {
            method: "POST",
            body: formDataObj
        });

        // auth objekt
        const loggedAccount = useState<Account | null>("loggedAccount", () => null);
        loggedAccount.value = res;

        // presmerovani na dashboard
        await navigateTo("/");
    } catch (err: any) {
        errorMsg.value = ["Uživatelské jméno nebo e-mail už existuje."];
    } finally {
        isLoading.value = false;
    }
}
</script>

<template>
    <Head>
        <Title>Register • Think different Academy</Title>
    </Head>

    <CircleBlurBlob top="10vw" left="-10vw" blur="10vw" color="var(--accent-color-secondary-theme)" />
    <CircleBlurBlob bottom="10vw" right="-10vw" blur="10vw" color="var(--accent-color-primary)" />

    <main :class="$style.page">
        <section :class="[$style.card]" aria-labelledby="login-title">
            <header :class="$style.header">
                <h1 id="login-title" :class="[$style.title, 'text-gradient']">Zaregistrovat se.</h1>
                <p :class="$style.subtitle">Think different Academy</p>
            </header>

            <form :class="[$style.form]" @submit.prevent="submitRegisterForm" aria-describedby="form-error" :aria-busy="isLoading">
                <div :class="$style.group">
                    <label :class="$style.label" for="username">Uživatelské jméno</label>
                    <Input
                        id="username"
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
                    <label :class="$style.label" for="email">E-mailová adresa</label>
                    <Input
                        v-model="email"
                        id="email"
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
                    <label :class="$style.label" for="passwordApprove">Heslo</label>
                    <div :class="$style.passwordRow">
                        <Input
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
                            @click="togglePassword"
                            :aria-pressed="showPassword"
                            aria-controls="password"
                            aria-label="Zobrazit nebo skrýt heslo"
                        >
                            {{ showPassword ? 'Skrýt' : 'Zobrazit' }}
                        </button>
                    </div>
                </div>
                <div :class="$style.group">
                    <label :class="$style.label" for="password">Potvrzení hesla</label>
                    <div :class="$style.passwordRow">
                        <Input
                            v-model="passwordApprove"
                            style="width: 100%"
                            placeholder="••••••••"
                            aria-describedby="password-help"
                            required
                            name="password"
                            :type="showPasswordApprove ? 'text' : 'password'"
                        />
                        <button
                            :class="$style.toggle"
                            type="button"
                            @click="togglePasswordApprove"
                            :aria-pressed="showPasswordApprove"
                            aria-controls="password"
                            aria-label="Zobrazit nebo skrýt heslo"
                        >
                            {{ showPasswordApprove ? 'Skrýt' : 'Zobrazit' }}
                        </button>
                    </div>
                </div>


                <ButtonComponent button-style="primary" accent-color="primary" :class="$style.button" type="submit" :loading="isLoading">
                    Zaregistrovat se
                </ButtonComponent>

                <div :class="$style.errorGrid">

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
            
            
        </section>
    </main>
</template>

<style module lang="scss">
/* util */
.visuallyHidden {
    /* skryje text pro zrak, ponecha pro ctecky */
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0 0 0 0);
    white-space: nowrap;
    border: 0;
}

/* page layout */
.page {
    display: grid;
    place-items: center;
    padding: 24px;
}

/* card */
.card {
    width: 100%;
    max-width: 420px;
    border-radius: 20px;
    padding: 28px;
}

/* header */
.header {
    text-align: center;
    margin-bottom: 18px;
}
.title {
    font-size: 28px;
    line-height: 1.2;
    letter-spacing: -0.02em;
    margin: 0 auto;
    margin-bottom: 4px;
    width: fit-content;
}
.subtitle {
    margin: 0;
    opacity: 0.75;
    font-size: 14px;
}

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

.error {
    display: flex;
    gap: 6px;
    flex-direction: column;
    color: var(--accent-color-primary);
    width: fit-content;
    font-size: 14px;
    font-weight: 700;
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

.input {
    width: 100%;
    height: 44px;
    padding: 10px 14px;
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.22);
    background: rgba(10, 12, 20, 0.5);
    color: inherit;
    outline: none;
    transition: border-color 0.2s ease, box-shadow 0.2s ease, background 0.2s ease;

    @media (prefers-color-scheme: light) {
        background: #ffffff;
        border-color: rgba(15, 18, 34, 0.12);
    }

    &:hover {
        border-color: rgba(99, 102, 241, 0.6);
    }
    &:focus-visible {
        border-color: #6366f1;
        box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.25);
        background: rgba(10, 12, 20, 0.6);

        @media (prefers-color-scheme: light) {
            background: #ffffff;
        }
    }
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

.isLoading {
    /* pri loadingu misto textu ukaz spinner */
    position: relative;
    color: transparent;
}

.spinner {
    /* jednoduchy spinner */
    width: 20px;
    height: 20px;
    display: inline-block;
    border-radius: 50%;
    border: 2px solid currentColor;
    border-right-color: transparent;
    animation: spin 0.7s linear infinite;
    vertical-align: middle;
}

@keyframes spin {
    to { transform: rotate(360deg); }
}

/* footer links pod tlacitkem */
.actions {
    display: flex;
    justify-content: center;
    margin-top: 6px;

    a {
        font-size: 13px;
        opacity: 0.85;
        text-decoration: none;
        transition: opacity 0.2s ease, color 0.2s ease;

        &:hover { opacity: 1; color: #6366f1; }
    }
}
</style>
