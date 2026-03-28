<script setup lang="ts">
import { push } from "notivue";
import type { Account, Organization } from "#shared/types";
import getBaseUrl from "#shared/utils/getBaseUrl";
import Button from "~/components/Button.vue";
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
const selectedOrganization = ref<Organization | null>(null);
const isDeleteModalOpen = ref(false);

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

function editOrganization(_: Organization): void {
    // TODO: edit flow will be implemented later.
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
</script>

<template>
    <Head>
        <Title>Pobočky • Think different Academy</Title>
    </Head>

    <section :class="$style.page">
        <h1 :class="$style.heading">Pobočka</h1>
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

}
</style>
