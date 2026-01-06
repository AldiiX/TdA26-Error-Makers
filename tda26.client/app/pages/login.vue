<script setup lang="ts">
import { ref } from "vue";
import BlurBackground from "~/components/backgrounds/BlurBackground.vue";
import CircleBlurBlob from "~/components/CircleBlurBlob.vue";
import ButtonComponent from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import type {Account} from "#shared/types";
import { push } from "notivue";

definePageMeta({
    layout: "normal-page-layout"
});

// SEO
useSeo({
    title: "Přihlášení",
    description: "Přihlaste se do svého účtu na Think Different Academy a pokračujte ve vzdělávání. Získejte přístup ke svým kurzům a sledujte svůj pokrok.",
    keywords: "přihlášení, login, účet, vstup do systému",
    noindex: true // Login pages shouldn't be indexed
});

const loggedAccount = useState<Account | null>("loggedAccount", () => null);
if (loggedAccount.value) {
    navigateTo("/");
}

// state
const showPassword = ref(false);
const password = ref("");
const isLoading = ref(false);
const errorMsg = ref<string | null>(null);

// handlers
function togglePassword() {
    // prepina zobrazeni hesla
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
        const res = await $fetch<Account | null>("/api/v2/auth/login", {
            method: "POST",
            body: formDataObj
        });

        if (!res) {
            throw new Error("Invalid credentials");
        }

        const loggedAccount = useState<Account | null>("loggedAccount", () => null);
        loggedAccount.value = res;

        loginToast.resolve({
            message: "Úspěšně přihlášeno! Přesměrovávám...",
            duration: 1200
        });

        await navigateTo("/");
    } catch (err: any) {
        errorMsg.value = "Nesprávné uživatelské jméno nebo heslo.";

        // prepnuti toastu na error (a nechat chvili zobrazeny)
        loginToast.resolve({
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
    <Head>
        <Title>Login • Think different Academy</Title>
    </Head>

    <CircleBlurBlob top="10vw" left="-10vw" blur="10vw" color="var(--accent-color-secondary-theme)" />
    <CircleBlurBlob bottom="10vw" right="-10vw" blur="10vw" color="var(--accent-color-primary)" />

    <main :class="$style.page">
        <section :class="[$style.card]" aria-labelledby="login-title">
            <header :class="$style.header">
                <h1 id="login-title" :class="[$style.title, 'text-gradient']">Přihlášení</h1>
                <p :class="$style.subtitle">Think different Academy</p>
            </header>

            <form :class="[$style.form]" @submit.prevent="submitLoginForm" aria-describedby="form-error" :aria-busy="isLoading">
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
                    <label :class="$style.label" for="password">Heslo</label>
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


                <ButtonComponent button-style="primary" accent-color="primary" :class="$style.button" type="submit" :loading="isLoading">
                    Přihlásit se
                </ButtonComponent>

                <p v-if="errorMsg" :class="$style.error" id="form-error" role="alert" aria-live="polite">{{ errorMsg }}</p>

                <div :class="$style.actions">
<!--                    <a href="/forgot-password">Zapomněl jsi heslo?</a>-->
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
