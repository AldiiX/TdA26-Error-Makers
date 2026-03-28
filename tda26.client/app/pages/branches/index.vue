<script setup lang="ts">
import { push } from "notivue";
import type { Account, Organization, OrganizationRegion, OrganizationStatus, OrganizationType } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
import Input from "~/components/Input.vue";
import Modal from "~/components/Modal.vue";
import ModalDestructive from "~/components/ModalDestructive.vue";

definePageMeta({
    layout: "normal-page-layout",
    middleware: () => {
        const user = useState<Account | null>("loggedAccount");
        if (!user.value) return navigateTo("/login");
        if (user.value.type !== "admin") return navigateTo("/");
    }
});

useSeo({
    title: "Organizace",
    description: "Přehled organizací v administraci.",
    noindex: true
});

const organizations = ref<Organization[]>([]);
const isLoading = ref(true);
const loadError = ref<string | null>(null);
const deleting = ref(false);
const savingEdit = ref(false);
const selectedOrganization = ref<Organization | null>(null);
const isDeleteModalOpen = ref(false);
const isEditModalOpen = ref(false);
const editError = ref<string | null>(null);

const editingOrganization = reactive({
    uuid: "",
    displayName: "",
    country: "",
    city: "",
    address: "",
    postalCode: "",
    region: "centralEurope" as OrganizationRegion,
    type: "hq" as OrganizationType,
    status: "onboarding" as OrganizationStatus
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

const loadOrganizations = async (): Promise<void> => {
    isLoading.value = true;
    loadError.value = null;

    try {
        organizations.value = await $fetch<Organization[]>(`${getBaseUrl()}/api/v1/organizations`, {
            method: "GET"
        });
    } catch {
        loadError.value = "Nepodařilo se načíst organizace.";
        organizations.value = [];
    } finally {
        isLoading.value = false;
    }
};

await loadOrganizations();

function editOrganization(organization: Organization): void {
    editError.value = null;
    editingOrganization.uuid = organization.uuid;
    editingOrganization.displayName = organization.displayName;
    editingOrganization.country = organization.country;
    editingOrganization.city = organization.city;
    editingOrganization.address = organization.address;
    editingOrganization.postalCode = organization.postalCode;
    editingOrganization.region = organization.region;
    editingOrganization.type = organization.type;
    editingOrganization.status = organization.status;
    isEditModalOpen.value = true;
}

function openDeleteModal(organization: Organization): void {
    selectedOrganization.value = organization;
    isDeleteModalOpen.value = true;
}

async function deleteOrganization(): Promise<void> {
    if (!selectedOrganization.value || deleting.value) return;

    deleting.value = true;

    try {
        await $fetch(`${getBaseUrl()}/api/v1/organizations/${selectedOrganization.value.uuid}`, {
            method: "DELETE"
        });

        organizations.value = organizations.value.filter(o => o.uuid !== selectedOrganization.value?.uuid);
        push.success({
            title: "Organizace smazána",
            message: "Organizace byla úspěšně smazána.",
            duration: 4000
        });

        isDeleteModalOpen.value = false;
        selectedOrganization.value = null;
    } catch {
        push.error({
            title: "Mazání selhalo",
            message: "Organizaci se nepodařilo smazat.",
            duration: 5000
        });
    } finally {
        deleting.value = false;
    }
}

async function saveOrganizationEdit(): Promise<void> {
    if (!editingOrganization.uuid || savingEdit.value) return;

    editError.value = null;
    const payload = {
        displayName: editingOrganization.displayName.trim(),
        country: editingOrganization.country.trim(),
        city: editingOrganization.city.trim(),
        address: editingOrganization.address.trim(),
        postalCode: editingOrganization.postalCode.trim(),
        region: editingOrganization.region,
        type: editingOrganization.type,
        status: editingOrganization.status
    };

    if (!payload.displayName || !payload.country || !payload.city || !payload.address || !payload.postalCode) {
        editError.value = "Vyplň všechna povinná pole.";
        return;
    }

    savingEdit.value = true;

    try {
        const updated = await $fetch<Organization>(`${getBaseUrl()}/api/v1/organizations/${editingOrganization.uuid}`, {
            method: "PUT",
            body: payload
        });

        organizations.value = organizations.value.map(org => org.uuid === updated.uuid ? updated : org);
        isEditModalOpen.value = false;
        push.success({
            title: "Pobočka upravena",
            message: "Změny byly úspěšně uloženy.",
            duration: 4000
        });
    } catch {
        editError.value = "Nepodařilo se uložit změny pobočky.";
        push.error({
            title: "Uložení selhalo",
            message: "API vrátilo chybu při úpravě pobočky.",
            duration: 5000
        });
    } finally {
        savingEdit.value = false;
    }
}
</script>

<template>
    <Head>
        <Title>Pobočky • Think different Academy</Title>
    </Head>

    <section :class="$style.page">
        <h1 :class="$style.heading">Pobočky</h1>
        <p :class="$style.description">Seznam všech organizací a jejich základních informací.</p>

        <div v-if="isLoading" :class="$style.info">Načítání poboček...</div>
        <div v-else-if="loadError" :class="[$style.info, $style.error]">{{ loadError }}</div>
        <div v-else-if="organizations.length === 0" :class="$style.info">Žádné po zatím nejsou vytvořeny.</div>

        <div v-else :class="$style.tableWrap">
            <div :class="$style.tableHeader">
                <p>Jméno</p>
                <p>Adresa</p>
                <p>Oblast</p>
                <p>Typ</p>
                <p>Status</p>
                <p>Lektoři</p>
                <p>Studenti</p>
                <p :class="$style.actionsHeader">Actions</p>
            </div>

            <article v-for="organization in organizations" :key="organization.uuid" :class="$style.tableRow">
                <p :class="$style.name">{{ organization.displayName }}</p>
                <p>{{ organization.country }}, {{ organization.city }}<br>{{ organization.address }}, {{ organization.postalCode }}</p>
                <p>{{ organization.region }}</p>
                <p>{{ organization.type }}</p>
                <p>{{ organization.status }}</p>
                <p>{{ organization.lecturerUuids.length }}</p>
                <p>{{ organization.studentUuids.length }}</p>

                <div :class="$style.actions">
                    <Button button-style="secondary" @click="editOrganization(organization)">
                        Edit
                    </Button>
                    <Button button-style="primary" accent-color="primary" @click="openDeleteModal(organization)">
                        Delete
                    </Button>
                </div>
            </article>
        </div>
    </section>

    <Teleport to="#teleports">
        <Modal
            :enabled="isEditModalOpen"
            :can-be-closed-by-clicking-outside="true"
            :modal-style="{ maxWidth: '760px' }"
            @close="isEditModalOpen = false"
        >
            <h2 :class="$style.modalTitle">Upravit pobočku</h2>

            <form :class="$style.editForm" @submit.prevent="saveOrganizationEdit">
                <div :class="$style.group">
                    <label for="displayName">Jméno</label>
                    <Input id="displayName" v-model="editingOrganization.displayName" required />
                </div>

                <div :class="$style.groupRow">
                    <div :class="$style.group">
                        <label for="country">Země</label>
                        <Input id="country" v-model="editingOrganization.country" required />
                    </div>

                    <div :class="$style.group">
                        <label for="city">Město</label>
                        <Input id="city" v-model="editingOrganization.city" required />
                    </div>
                </div>

                <div :class="$style.groupRow">
                    <div :class="$style.group">
                        <label for="address">Adresa</label>
                        <Input id="address" v-model="editingOrganization.address" required />
                    </div>

                    <div :class="$style.group">
                        <label for="postalCode">PSČ</label>
                        <Input id="postalCode" v-model="editingOrganization.postalCode" required />
                    </div>
                </div>

                <div :class="$style.groupRow3">
                    <div :class="$style.group">
                        <label for="region">Oblast</label>
                        <Input id="region" v-model="editingOrganization.region" type="select" required>
                            <option v-for="option in regionOptions" :key="option.value" :value="option.value">
                                {{ option.label }}
                            </option>
                        </Input>
                    </div>

                    <div :class="$style.group">
                        <label for="type">Typ</label>
                        <Input id="type" v-model="editingOrganization.type" type="select" required>
                            <option v-for="option in typeOptions" :key="option.value" :value="option.value">
                                {{ option.label }}
                            </option>
                        </Input>
                    </div>

                    <div :class="$style.group">
                        <label for="status">Status</label>
                        <Input id="status" v-model="editingOrganization.status" type="select" required>
                            <option v-for="option in statusOptions" :key="option.value" :value="option.value">
                                {{ option.label }}
                            </option>
                        </Input>
                    </div>
                </div>

                <p v-if="editError" :class="$style.errorText">{{ editError }}</p>

                <div :class="$style.modalActions">
                    <Button button-style="secondary" @click="isEditModalOpen = false">
                        Zrušit
                    </Button>
                    <Button button-style="primary" accent-color="primary" :loading="savingEdit" :disabled="savingEdit">
                        Uložit změny
                    </Button>
                </div>
            </form>
        </Modal>

        <ModalDestructive
            :enabled="isDeleteModalOpen"
            :title="'Smazat organizaci?'"
            :description="`Opravdu chceš smazat organizaci ${selectedOrganization?.displayName ?? ''}? Tuto akci nelze vrátit zpět.`"
            :yes-text="deleting ? 'Mazání...' : 'Smazat organizaci'"
            :yes-action="deleteOrganization"
            :no-action="() => { isDeleteModalOpen = false; selectedOrganization = null; }"
            @close="() => { isDeleteModalOpen = false; selectedOrganization = null; }"
        />
    </Teleport>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.page {
    max-width: 1100px;
}

.heading {
    margin: 0;
    font-size: 48px;
}

.description {
    margin-top: 12px;
    color: var(--text-color-secondary);
}

.info {
    margin-top: 24px;
    padding: 16px;
    border-radius: 12px;
    background: var(--background-color-2);
}

.error {
    color: var(--accent-color-secondary-theme);
}

.tableWrap {
    margin-top: 24px;
    display: grid;
    gap: 10px;
}

.tableHeader,
.tableRow {
    display: grid;
    grid-template-columns: 1.3fr 1.8fr 1fr 0.8fr 0.9fr 0.7fr 0.8fr 1.2fr;
    gap: 10px;
    align-items: center;
    padding: 14px 16px;
    border-radius: 12px;
}

.tableHeader {
    background: color-mix(in srgb, var(--background-color-2) 75%, transparent);
    color: var(--text-color-secondary);
    font-size: 13px;
    font-weight: 600;

    p {
        margin: 0;
    }
}

.tableRow {
    background: var(--background-color-2);
    box-shadow: var(--button-shadow);

    p {
        margin: 0;
        overflow-wrap: anywhere;
        line-height: 1.3;
    }
}

.name {
    font-weight: 700;
}

.actionsHeader {
    text-align: right;
}

.actions {
    display: flex;
    gap: 12px;
    justify-content: flex-end;
}

.modalTitle {
    margin: 0;
    margin-bottom: 12px;
}

.editForm {
    display: grid;
    gap: 12px;
}

.group,
.groupRow,
.groupRow3 {
    > .group {
        display: grid;
        gap: 8px;
    }

    label {
        color: var(--text-color-secondary);
        font-size: 14px;
    }
}

.group {
    display: grid;
    gap: 8px;
}

.groupRow {
    display: grid;
    gap: 12px;
    grid-template-columns: repeat(2, minmax(0, 1fr));
}

.groupRow3 {
    display: grid;
    gap: 12px;
    grid-template-columns: repeat(3, minmax(0, 1fr));
}

.errorText {
    margin: 0;
    color: var(--accent-color-secondary-theme);
}

.modalActions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}

@media screen and (max-width: app.$tabletBreakpoint) {
    .heading {
        font-size: 36px;
    }

    .tableHeader {
        display: none;
    }

    .tableRow {
        grid-template-columns: 1fr;
    }

    .actions {
        justify-content: stretch;

        > button {
            flex: 1;
        }
    }

    .groupRow,
    .groupRow3 {
        grid-template-columns: 1fr;
    }

    .modalActions {
        > button {
            flex: 1;
        }
    }

}
</style>
