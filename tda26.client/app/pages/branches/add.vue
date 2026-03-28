<script setup lang="ts">
import { push } from "notivue";
import type {
    Account,
    CreateOrganizationPayload,
    Organization,
    OrganizationRegion,
    OrganizationStatus,
    OrganizationType
} from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import Modal from "~/components/Modal.vue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>("loggedAccount");
        if (!user.value) return navigateTo("/login");
        if (user.value.type !== "admin") return navigateTo("/");
    }
});

useSeo({
    title: "Přidat organizaci",
    description: "Administrace organizací v Think Different Academy.",
    noindex: true
});

const isSubmitting = ref(false);
const isInviteModalOpen = ref(false);
const submitError = ref<string | null>(null);
const placeholderRegistrationLink = ref("");
const copied = ref(false);

const form = reactive({
    displayName: "",
    country: "",
    city: "",
    address: "",
    postalCode: "",
    region: "centralEurope" as OrganizationRegion,
    type: "hq" as OrganizationType,
    status: "onboarding" as OrganizationStatus,
    registrationEmail: ""
});

const regionOptions: { value: OrganizationRegion; label: string }[] = [
    { value: "centralEurope", label: "Central Europe" },
    { value: "europe", label: "Europe" },
    { value: "northAmerica", label: "North America" },
    { value: "southAmerica", label: "South America" },
    { value: "asia", label: "Asia" },
    { value: "africa", label: "Africa" },
    { value: "oceania", label: "Oceania" },
    { value: "middleEast", label: "Middle East" },
    { value: "global", label: "Global" }
];

const typeOptions: { value: OrganizationType; label: string }[] = [
    { value: "hq", label: "HQ" },
    { value: "branch", label: "Branch" }
];

const statusOptions: { value: OrganizationStatus; label: string }[] = [
    { value: "active", label: "Active" },
    { value: "onboarding", label: "Onboarding" },
    { value: "waiting", label: "Waiting" }
];

function isValidEmail(value: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}

function resetForm(): void {
    form.displayName = "";
    form.country = "";
    form.city = "";
    form.address = "";
    form.postalCode = "";
    form.region = "centralEurope";
    form.type = "hq";
    form.status = "onboarding";
    form.registrationEmail = "";
}

async function submitForm(): Promise<void> {
    submitError.value = null;
    copied.value = false;

    const trimmedEmail = form.registrationEmail.trim();
    if (!isValidEmail(trimmedEmail)) {
        submitError.value = "E-mail pro registrační instrukce není ve správném formátu.";
        return;
    }

    const payload: CreateOrganizationPayload = {
        displayName: form.displayName.trim(),
        country: form.country.trim(),
        city: form.city.trim(),
        address: form.address.trim(),
        postalCode: form.postalCode.trim(),
        region: form.region,
        type: form.type,
        status: form.status,
        lecturerUuids: [],
        studentUuids: []
    };

    if (!payload.displayName || !payload.country || !payload.city || !payload.address || !payload.postalCode) {
        submitError.value = "Vyplň všechna povinná pole organizace.";
        return;
    }

    isSubmitting.value = true;

    try {
        const created = await $fetch<Organization>(`${getBaseUrl()}/api/v1/organizations`, {
            method: "POST",
            body: payload
        });

        const origin = process.client ? window.location.origin : "";
        placeholderRegistrationLink.value = `${origin}/register?organization=${encodeURIComponent(created.uuid)}&email=${encodeURIComponent(trimmedEmail)}`;

        isInviteModalOpen.value = true;
        push.success({
            title: "Organizace vytvořena",
            message: "Organizace byla úspěšně vytvořena.",
            duration: 4500
        });

        resetForm();
    } catch (error) {
        submitError.value = "Nepodařilo se vytvořit organizaci. Zkus to prosím znovu.";
        push.error({
            title: "Chyba při vytváření",
            message: "API vrátilo chybu při ukládání organizace.",
            duration: 5000
        });
    } finally {
        isSubmitting.value = false;
    }
}

async function copyInviteLink(): Promise<void> {
    if (!placeholderRegistrationLink.value || !process.client) return;

    try {
        await navigator.clipboard.writeText(placeholderRegistrationLink.value);
        copied.value = true;
        push.success({
            title: "Odkaz zkopírován",
            message: "Placeholder odkaz byl zkopírován do schránky.",
            duration: 3000
        });
    } catch {
        push.error({
            title: "Kopírování selhalo",
            message: "Odkaz se nepodařilo zkopírovat.",
            duration: 3500
        });
    }
}
</script>

<template>
    <Head>
        <Title>Přidat organizaci • Think different Academy</Title>
    </Head>

    <section :class="$style.page">
        <h1 :class="$style.heading">Přidat organizaci</h1>
        <p :class="$style.description">
            Tato stránka je dostupná pouze pro administrátory. Po vytvoření organizace zobrazíme placeholder odkaz,
            na který by se v budoucnu poslaly registrační instrukce.
        </p>

        <form :class="$style.form" @submit.prevent="submitForm">
            <div :class="$style.group">
                <label for="displayName">Display name</label>
                <Input id="displayName" v-model="form.displayName" required placeholder="Think Different Academy" />
            </div>

            <div :class="$style.groupRow">
                <div :class="$style.group">
                    <label for="country">Country</label>
                    <Input id="country" v-model="form.country" required placeholder="Czech Republic" />
                </div>

                <div :class="$style.group">
                    <label for="city">City</label>
                    <Input id="city" v-model="form.city" required placeholder="Brno" />
                </div>
            </div>

            <div :class="$style.groupRow">
                <div :class="$style.group">
                    <label for="address">Address</label>
                    <Input id="address" v-model="form.address" required placeholder="Masarykova 1" />
                </div>

                <div :class="$style.group">
                    <label for="postalCode">Postal code</label>
                    <Input id="postalCode" v-model="form.postalCode" required placeholder="602 00" />
                </div>
            </div>

            <div :class="$style.groupRow">
                <div :class="$style.group">
                    <label for="region">Region</label>
                    <Input id="region" v-model="form.region" type="select" required>
                        <option v-for="option in regionOptions" :key="option.value" :value="option.value">
                            {{ option.label }}
                        </option>
                    </Input>
                </div>

                <div :class="$style.group">
                    <label for="type">Type</label>
                    <Input id="type" v-model="form.type" type="select" required>
                        <option v-for="option in typeOptions" :key="option.value" :value="option.value">
                            {{ option.label }}
                        </option>
                    </Input>
                </div>

                <div :class="$style.group">
                    <label for="status">Status</label>
                    <Input id="status" v-model="form.status" type="select" required>
                        <option v-for="option in statusOptions" :key="option.value" :value="option.value">
                            {{ option.label }}
                        </option>
                    </Input>
                </div>
            </div>

            <div :class="$style.group">
                <label for="registrationEmail">E-mail pro zaslání registračních instrukcí</label>
                <Input
                    id="registrationEmail"
                    v-model="form.registrationEmail"
                    required
                    type="email"
                    placeholder="admin@organization.com"
                />
            </div>

            <p v-if="submitError" :class="$style.error">{{ submitError }}</p>

            <div :class="$style.submitWrap">
                <Button
                    button-style="primary"
                    accent-color="primary"
                    :loading="isSubmitting"
                    :disabled="isSubmitting"
                >
                    Vytvořit organizaci
                </Button>
            </div>
        </form>
    </section>

    <Teleport to="#teleports">
        <Modal
            :enabled="isInviteModalOpen"
            :can-be-closed-by-clicking-outside="true"
            :modal-style="{ maxWidth: '680px' }"
            @close="isInviteModalOpen = false"
        >
            <h2 :class="$style.modalTitle">Placeholder registrační odkaz</h2>
            <p :class="$style.modalDescription">
                E-mail se zatím neposílá. Níže je placeholder odkaz, který můžeš zatím ručně použít.
            </p>

            <div :class="$style.linkBox">
                <input :value="placeholderRegistrationLink" readonly :class="$style.linkInput">
            </div>

            <div :class="$style.modalActions">
                <Button button-style="secondary" @click="copyInviteLink">
                    {{ copied ? "Zkopírováno" : "Kopírovat odkaz" }}
                </Button>
                <Button button-style="primary" accent-color="primary" @click="isInviteModalOpen = false">
                    Zavřít
                </Button>
            </div>
        </Modal>
    </Teleport>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.page {
    max-width: 960px;
}

.heading {
    margin: 0;
    font-size: 48px;
}

.description {
    margin-top: 12px;
    color: var(--text-color-secondary);
}

.form {
    margin-top: 28px;
    display: grid;
    gap: 16px;
    padding: 24px;
    border-radius: 16px;
    background: var(--background-color-2);
}

.group {
    display: grid;
    gap: 8px;

    > label {
        font-size: 14px;
        color: var(--text-color-secondary);
    }
}

.groupRow {
    display: grid;
    gap: 16px;
    grid-template-columns: repeat(2, minmax(0, 1fr));
}

.groupRow:nth-of-type(4) {
    grid-template-columns: repeat(3, minmax(0, 1fr));
}

.error {
    color: var(--accent-color-secondary-theme);
    margin: 0;
}

.submitWrap {
    margin-top: 6px;
}

.modalTitle {
    margin: 0;
    margin-bottom: 8px;
}

.modalDescription {
    margin-top: 0;
    color: var(--text-color-secondary);
}

.linkBox {
    margin-top: 16px;
}

.linkInput {
    width: 100%;
    border-radius: 10px;
    border: 1px solid var(--input-border-color);
    background: var(--input-background-color);
    color: var(--text-color);
    padding: 12px;
    font-family: Dosis, sans-serif;
}

.modalActions {
    margin-top: 16px;
    display: flex;
    gap: 12px;
    justify-content: flex-end;
}

@media screen and (max-width: app.$tabletBreakpoint) {
    .heading {
        font-size: 36px;
    }

    .groupRow,
    .groupRow:nth-of-type(4) {
        grid-template-columns: 1fr;
    }

    .modalActions {
        justify-content: stretch;

        > button {
            flex: 1;
        }
    }
}
</style>
