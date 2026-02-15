<script setup lang="ts">
import ContextMenuItem from "./ContextMenuItem.vue"

export interface ContextMenuItemType {
    text: string
    iconPath?: string
    onClick?: () => void
    href?: string
    disabled?: boolean
    forceLoad?: boolean
}

const props = defineProps<{
    items: ContextMenuItemType[]
    x: number
    y: number
    visible: boolean
}>()

const emit = defineEmits<{
    (e: "close"): void
}>()

const menuRef = ref<HTMLElement | null>(null)
const zoneRef = ref<HTMLElement | null>(null)

function onDocumentClick(event: MouseEvent) {
    if (!menuRef.value) return

    if (!menuRef.value.contains(event.target as Node) || (zoneRef.value && zoneRef.value.contains(event.target as Node))) {
        emit("close")
    }
}

function onDocumentScroll() {
    emit("close")
}

function handleItemClick(item: ContextMenuItemType) {
    if (item.disabled) return

    emit("close")
    item.onClick?.()
}

function handleLinkClick(item: ContextMenuItemType) {
    if (item.disabled) return

    if (item.forceLoad && item.href) {
        window.location.href = item.href
    } else {
        handleItemClick(item)
    }
}

onMounted(() => {
    document.addEventListener("mousedown", onDocumentClick)
    document.addEventListener("scroll", onDocumentScroll, { passive: true })
})

onBeforeUnmount(() => {
    document.removeEventListener("mousedown", onDocumentClick)
    document.removeEventListener("scroll", onDocumentScroll)
})
</script>

<template>
    <teleport to="body">
        <Transition name="context-menu-fade">
            <div 
                v-if="visible"
                ref="menuRef"
                :class="[$style.contextMenu, 'liquid-glass']"
                :style="{ top: `${y}px`, left: `${x}px` }"
            >
                <div ref="zoneRef" :class="$style.zone"/>

                <template v-for="(item, index) in items" :key="index">
                    <ContextMenuItem
                        v-if="!item.href && !item.forceLoad"
                        :item="item"
                        @click="handleItemClick"
                    />
    
                    <a
                        v-else
                        :href="item.href"
                        @click.prevent="handleLinkClick(item)"
                    >
                        <ContextMenuItem
                            :item="item"
                            @click="handleItemClick"
                        />
                    </a>
    
                    <hr v-if="index !== items.length - 1">
                </template>
            </div>
        </Transition>
    </teleport>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

.contextMenu {
    position: fixed;
    top: 0;
    left: 0;
    min-width: 200px;
    padding: 8px;
    //background: var(--background-color-secondary);
    background: rgb(from var(--background-color-secondary) r g b/ 0.75);
    border: 1px solid var(--input-border-color);
    border-radius: 12px;
    z-index: 10;
    //box-shadow: 0 4px 16px rgba(0, 0, 0, 0.5);

    a {
        color: inherit;
        text-decoration: none;
    }

    hr {
        margin: 8px 0;
        border: none;
        border-top: 1px solid var(--input-border-color);
        opacity: .5;
    }
}

.zone {
    position: absolute;
    width: 100%;
    height: 100%;
    top: 050%;
    left: 050%;
    transform: translate(-50%, -50%);
    padding: 32px;
    box-sizing: content-box;
    z-index: -1;
}

</style>

<style lang="scss" scoped>
.context-menu-fade-enter-active,
.context-menu-fade-leave-active {
    transition:
        opacity 120ms ease,
        transform 120ms ease;
}

.context-menu-fade-enter-from,
.context-menu-fade-leave-to {
    opacity: 0;
    transform: scale(0.96);
}

.context-menu-fade-enter-to,
.context-menu-fade-leave-from {
    opacity: 1;
    transform: scale(1);
}
</style>