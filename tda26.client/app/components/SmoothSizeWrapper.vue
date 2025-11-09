<script setup lang="ts">
import { onMounted, onBeforeUnmount, shallowRef, ref, computed, nextTick, watchEffect } from "vue";
import type { Component } from "vue";

type Emits = {
    (e: "resize", payload: { width: number; height: number }): void;
};

const emit = defineEmits<Emits>();

const props = withDefaults(defineProps<{
    /** delka prechodu v ms */
    transitionDuration?: number;
    /** jestli animovat vysku */
    changeHeight?: boolean;
    /** jestli animovat sirku */
    changeWidth?: boolean;
    /** css easing funkce */
    easing?: string;
    /** renderovaci tag/komponenta obaloveho elementu */
    tag?: keyof HTMLElementTagNameMap | Component;
    /** animovat i prvotni mereni (na mount)? */
    animateInitial?: boolean;
}>(), {
    transitionDuration: 500,
    changeHeight: true,
    changeWidth: true,
    easing: "ease",
    tag: "div",
    animateInitial: true
});

const wrapperRef = shallowRef<HTMLElement | null>(null);
const contentRef = shallowRef<HTMLElement | null>(null);

// interni stav velikosti v pixelech
const targetWidth = ref<number>(0);
const targetHeight = ref<number>(0);
const hasMeasured = ref<boolean>(false);

let ro: ResizeObserver | null = null;

const transitionStyle = computed(() => {
    // slozim transition jen pro aktivni osy
    const parts: string[] = [];
    if (props.changeHeight) { parts.push(`height ${props.transitionDuration}ms ${props.easing}`); }
    if (props.changeWidth) { parts.push(`width ${props.transitionDuration}ms ${props.easing}`); }
    return parts.join(", ");
});

// reakcni styl wrapperu – vyslovne px hodnoty (nelze plynule prechazet na 'auto')
const wrapperStyle = computed(() => {
    const style: Record<string, string> = {
        //overflow: "hidden"
    };
    if (props.changeHeight) { style.height = hasMeasured.value ? `${targetHeight.value}px` : "auto"; }
    if (props.changeWidth) { style.width = hasMeasured.value ? `${targetWidth.value}px` : "auto"; }
    // initial anim toggle
    style.transition = hasMeasured.value && props.animateInitial ? transitionStyle.value : "none";
    return style;
});

function observe() {
    // guard pro ssr
    if (!import.meta.client) { return; }
    if (!contentRef.value) { return; }

    // inicialni mereni bez trhane animace
    const rect = contentRef.value.getBoundingClientRect();
    targetWidth.value = Math.max(0, Math.round(rect.width));
    targetHeight.value = Math.max(0, Math.round(rect.height));
    hasMeasured.value = false;

    // pockam jeden tick, pak animace povolim
    nextTick(() => { hasMeasured.value = true; });

    // vytvorim resize observer pro obsah
    ro = new ResizeObserver((entries) => {
        // beru prvni zaznam; contentRect ma sirokou podporu
        const cr = entries[0]?.contentRect;
        if (!cr) { return; }

        const w = Math.max(0, Math.round(cr.width));
        const h = Math.max(0, Math.round(cr.height));

        // aktualizuji cilove rozmery; css transition se postara o plynulost
        if (props.changeWidth) { targetWidth.value = w; }
        if (props.changeHeight) { targetHeight.value = h; }

        emit("resize", { width: w, height: h });
    });

    ro.observe(contentRef.value);
}

onMounted(() => {
    observe();
});

onBeforeUnmount(() => {
    // uklid observeru
    if (ro) { ro.disconnect(); ro = null; }
});

// kdyz se prepnou priznaky os nebo delka prechodu, upravim transition
watchEffect(() => {
    if (!wrapperRef.value) { return; }
    // kdyz je animateInitial vypnute, drzim 'none' jen do prvniho mereni
    if (hasMeasured.value && props.animateInitial) {
        wrapperRef.value.style.transition = transitionStyle.value;
    } else {
        wrapperRef.value.style.transition = "none";
    }
});
</script>

<template>
    <component :is="props.tag" ref="wrapperRef" :style="wrapperStyle">
        <!--
          content jako vnoreny box, jeho prirozena velikost se meri pres resizeobserver
          display:block je predvybrane, aby vyska odrazela tok obsahu
        -->
        <div ref="contentRef" style="display:grid">
            <slot />
        </div>
    </component>
</template>