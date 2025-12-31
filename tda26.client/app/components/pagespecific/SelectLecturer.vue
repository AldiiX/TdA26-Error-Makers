<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import Avatar from "~/components/Avatar.vue";

interface SelectOption {
    value: string
    label: string
    pictureUrl?: string
}

const props = defineProps<{
    options: SelectOption[]
    modelValue: string | null
    placeholder?: string
    searchPlaceholder?: string
    dropdownClass?: string
    class?: string
}>()

const emit = defineEmits<{
    'update:modelValue': [value: string | null]
}>()

const isOpen = ref(false)
const searchQuery = ref('')
const selectRef = ref<HTMLElement | null>(null)

const filteredOptions = computed(() => {
    if (!searchQuery.value) {
        return props.options
    }

    const query = searchQuery.value.toLowerCase().normalize('NFD').replace(/\p{Diacritic}/gu, '')
    return props.options.filter(option => {
        const label = option.label.toLowerCase().normalize('NFD').replace(/\p{Diacritic}/gu, '')
        return label.includes(query)
    })
})

const selectedOption = computed(() => {
    return props.options.find(opt => opt.value === props.modelValue)
})

const displayText = computed(() => {
    return selectedOption.value?.label ?? props.placeholder ?? 'Vybrat...'
})

function toggleDropdown() {
    isOpen.value = !isOpen.value
    if (isOpen.value) {
        searchQuery.value = ''
    }
}

function selectOption(option: SelectOption) {
    emit('update:modelValue', option.value)
    isOpen.value = false
    searchQuery.value = ''
}

function clearSelection() {
    emit('update:modelValue', null)
    isOpen.value = false
    searchQuery.value = ''
}

function handleClickOutside(event: MouseEvent) {
    if (selectRef.value && !selectRef.value.contains(event.target as Node)) {
        isOpen.value = false
        searchQuery.value = ''
    }
}

onMounted(() => {
    document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
    document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
    <div ref="selectRef" :class="[$style.select, props.class]">
        <div :class="$style.selectTrigger" @click="toggleDropdown" :data-open="isOpen">

            <div v-if="selectedOption" :class="[$style.first, $style.flex]">
                <Avatar :name="selectedOption.label" :src="selectedOption.pictureUrl" :size="24" v-if="selectedOption.pictureUrl"/>
                <p :class="$style.selectText">{{ selectedOption.label }}</p>
            </div>

            <p v-else :class="[$style.first, $style.selectText, { [$style.placeholder]: !selectedOption }]">
                {{ displayText }}
            </p>

            <div :class="[$style.arrow, { [$style.open]: isOpen }]"></div>
            <div
                v-if="selectedOption"
                :class="$style.clearButton"
                @click.stop="clearSelection"
                title="Vymazat výběr"
            ></div>
        </div>

        <transition name="dropdown">
            <div v-if="isOpen" :class="[$style.dropdown, props.dropdownClass]">
                <div :class="$style.searchBar">
                    <div :class="$style.searchIcon"></div>
                    <input
                        type="text"
                        :placeholder="searchPlaceholder ?? 'Hledat...'"
                        v-model="searchQuery"
                        @click.stop
                    />
                </div>

                <div :class="$style.optionsList">
                    <div
                        v-for="option in filteredOptions"
                        :key="option.value"
                        :class="$style.option"
                        :data-selected="option.value === modelValue"
                        @click="selectOption(option)"
                    >
                        <Avatar :name="option.label" :src="option.pictureUrl ?? null" :size="24" />
                        <p>{{ option.label }}</p>
                    </div>

                    <p v-if="filteredOptions.length === 0" :class="$style.noResults">
                        Nenalezeny žádné výsledky
                    </p>
                </div>
            </div>
        </transition>
    </div>
</template>

<style module lang="scss">
.select {
    position: relative;
    width: 100%;
}

.selectTrigger {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    gap: 8px;
    background-color: var(--background-color-3);
    padding: 12px 16px;
    border-radius: 12px;
    cursor: pointer;
    user-select: none;
    transition: all 0.2s ease-in-out;
    position: relative;

    .first {
        margin-right: auto;

        &:is(.flex) {
            display: flex;
            align-items: center;
            gap: 8px;
        }
    }

    &:hover {
        background-color: var(--background-color-primary);
    }

    &[data-open="true"] {
        background-color: var(--background-color-primary);
    }
}

.selectText {
    flex: 1;
    font-size: 16px;
    color: var(--text-color-primary);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    margin: 0;

    &.placeholder {
        color: var(--text-color-secondary);
        opacity: 0.8;
    }
}

.arrow {
    width: 16px;
    height: 16px;
    background-color: var(--text-color-secondary);
    mask-image: url('../../../public/icons/arrow_down.svg');
    mask-size: contain;
    mask-repeat: no-repeat;
    mask-position: center;
    transition: transform 0.2s ease-in-out;
    flex-shrink: 0;

    &.open {
        transform: rotate(180deg);
    }
}

.clearButton {
    width: 16px;
    height: 16px;
    background-color: var(--text-color-secondary);
    mask-image: url('../../../public/icons/close.svg');
    mask-size: contain;
    mask-repeat: no-repeat;
    mask-position: center;
    cursor: pointer;
    transition: opacity 0.2s ease-in-out;
    flex-shrink: 0;
    opacity: 0.6;

    &:hover {
        opacity: 1;
    }
}

.dropdown {
    position: absolute;
    top: calc(100% + 8px);
    left: 0;
    right: 0;
    background-color: var(--background-color-secondary);
    border: 1px solid rgb(from var(--background-color-secondary) r g b / 1);
    border-radius: 12px;
    box-shadow: 0 4px 24px rgba(0, 0, 0, 0.15);
    backdrop-filter: blur(8px);
    z-index: 1000;
    max-height: 320px;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.searchBar {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 12px 16px;
    border-bottom: 1px solid var(--background-color-3);
    background-color: var(--background-color-primary);

    input {
        width: 100%;
        border: none;
        outline: none;
        font-size: 16px;
        color: var(--text-color-primary);
        background: transparent;
        font-family: Dosis, sans-serif;

        &::placeholder {
            color: var(--text-color-secondary);
            opacity: 0.8;
        }
    }
}

.searchIcon {
    mask-image: url('../../../public/icons/search.svg');
    mask-size: cover;
    mask-position: center;
    mask-repeat: no-repeat;
    width: 20px;
    height: 20px;
    background-color: var(--text-color-secondary);
    opacity: 0.8;
    flex-shrink: 0;
}

.optionsList {
    overflow-y: auto;
    max-height: 240px;

    &::-webkit-scrollbar {
        width: 8px;
    }

    &::-webkit-scrollbar-track {
        background: transparent;
    }

    &::-webkit-scrollbar-thumb {
        background: var(--scrollbar-color);
        border-radius: 4px;
    }
}

.option {
    padding: 12px 16px;
    cursor: pointer;
    transition: background-color 0.2s ease-in-out;
    font-size: 16px;
    color: var(--text-color-primary);
    margin: 0;
    display: flex;
    align-items: center;
    gap: 10px;


    p {
        margin: 0;
    }

    &:hover {
        background-color: var(--background-color-primary);
    }

    &[data-selected="true"] {
        background-color: var(--accent-color-primary);
        color: var(--accent-color-primary-text);
    }
}

.noResults {
    padding: 16px;
    text-align: center;
    color: var(--text-color-secondary);
    font-size: 14px;
}



// Mobile responsive
@media (max-width: 600px) {
    .dropdown {
        position: fixed;
        top: auto;
        bottom: 0;
        left: 0;
        right: 0;
        max-height: 60vh;
        border-radius: 16px 16px 0 0;
    }

    .selectTrigger {
        padding: 14px 16px;
    }

    .option {
        padding: 14px 16px;
        font-size: 16px;
    }
}
</style>

<style scoped lang="scss">
// Dropdown animation
.dropdown-enter-active,
.dropdown-leave-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
    opacity: 0;
    transform: translateY(-8px);
}
</style>
