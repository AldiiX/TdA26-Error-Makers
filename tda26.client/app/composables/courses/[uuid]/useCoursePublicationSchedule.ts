import type {Course, CourseStatus, TimeOption} from "#shared/types";
import type {CourseDetailModal} from "~/composables/courses/[uuid]/courseDetailTypes";
import { type Ref } from "vue";

function getLocalDateTimeString(): string {
    const now = new Date()
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset())
    return now.toISOString().slice(0, 16)
}



function pad2(n: number) {
    return String(n).padStart(2, "0");
}

function toLocalIsoDate(d: Date) {
    // YYYY-MM-DD v lokálním čase
    const local = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
    return local.toISOString().slice(0, 10);
}

function toLocalIsoTime(d: Date) {
    // HH:MM v lokálním čase
    const local = new Date(d.getTime() - d.getTimezoneOffset() * 60000);
    return local.toISOString().slice(11, 16);
}

function roundUpToNext5Minutes(date: Date) {
    const d = new Date(date);
    d.setSeconds(0);
    d.setMilliseconds(0);

    const m = d.getMinutes();
    const rounded = Math.ceil(m / 5) * 5;
    d.setMinutes(rounded);

    // když to přeteče do další hodiny, Date si poradí
    return d;
}

export default function (params: {
    enabledModal: Ref<CourseDetailModal>;
    course: Ref<Course>;
    updateError: Ref<string | null>;
    deleteError: Ref<string | null>;
    originalCourse: Ref<Course | null>;
}) {
    // --- UI state ---
    const isCustomDateEnabled = ref(false);

    const selectedDateIso = ref<string>(toLocalIsoDate(new Date()));

    const customDateIso = ref<string>(toLocalIsoDate(new Date()));

    const selectedTime = ref<string>(toLocalIsoTime(roundUpToNext5Minutes(new Date(Date.now() + 5 * 60000))));

    const scheduleError = ref<string | null>(null);

    const dateOptions = computed(() => {
        const base = new Date();
        base.setHours(0, 0, 0, 0);

        const fmtDow = new Intl.DateTimeFormat("cs-CZ", { weekday: "short" });
        const fmtMon = new Intl.DateTimeFormat("cs-CZ", { month: "short" });

        return Array.from({ length: 7 }).map((_, i) => {
            const d = new Date(base);
            d.setDate(base.getDate() + i);

            return {
                iso: toLocalIsoDate(d),
                dow: fmtDow.format(d),           // např. "po"
                dom: pad2(d.getDate()),          // "01"
                mon: fmtMon.format(d),           // "zář"
            };
        });
    });

    const minCustomDateIso = computed(() => toLocalIsoDate(new Date()));
    const maxCustomDateIso = computed(() => "2100-01-01");

    function toggleCustomDate() {
        isCustomDateEnabled.value = !isCustomDateEnabled.value;

        if (isCustomDateEnabled.value) {
            customDateIso.value = selectedDateIso.value;
        } else {
            const iso = customDateIso.value;
            if (dateOptions.value.some(d => d.iso === iso)) {
                selectedDateIso.value = iso;
            } else {
                selectedDateIso.value = dateOptions.value[0]?.iso ?? toLocalIsoDate(new Date());
            }
        }
    }

    const finalDateTime = computed<Date | null>(() => {
        scheduleError.value = null;

        const dateIso = isCustomDateEnabled.value ? customDateIso.value : selectedDateIso.value;
        if (!dateIso || !selectedTime.value) return null;

        const candidate = new Date(`${dateIso}T${selectedTime.value}:00`);

        if (isNaN(candidate.getTime())) return null;

        const min = new Date(Date.now() + 60_000);
        if (candidate.getTime() < min.getTime()) {
            scheduleError.value = "Zvolený datum a čas musí být v budoucnosti.";
            return null;
        }

        return candidate;
    });

    const formattedPublishTime = computed(() => formatDateTime(finalDateTime.value));

    watch(params.course, (newValue) => {
        if (newValue.status === "scheduled") {
            params.enabledModal.value = "schedulePublication";
        }
    });

    watch(params.enabledModal, (val) => {
        if (val === null) return;

        params.updateError.value = null;
        params.deleteError.value = null;
        scheduleError.value = null;

        if (val !== "schedulePublication") return;

        const base = roundUpToNext5Minutes(new Date(Date.now() + 5 * 60000));
        selectedTime.value = toLocalIsoTime(base);

        const existing = params.course.value.scheduledStart;
        if (existing) {
            const d = new Date(existing);
            if (!isNaN(d.getTime())) {
                const isoDate = toLocalIsoDate(d);
                const isoTime = toLocalIsoTime(d);

                selectedTime.value = isoTime;

                if (dateOptions.value.some(x => x.iso === isoDate)) {
                    isCustomDateEnabled.value = false;
                    selectedDateIso.value = isoDate;
                    customDateIso.value = isoDate;
                } else {
                    isCustomDateEnabled.value = true;
                    customDateIso.value = isoDate;
                    selectedDateIso.value = dateOptions.value[0]?.iso ?? toLocalIsoDate(new Date());
                }
            }
        }
    });

    async function confirmPublicationSchedule() {
        if (!finalDateTime.value || !params.course.value) return;

        await $fetch(`/api/v1/courses/${params.course.value.uuid}/status`, {
            method: "PUT",
            body: {
                status: "scheduled",
                scheduledStart: finalDateTime.value.toISOString(),
            },
        });

        params.enabledModal.value = null;
        params.course.value.status = "scheduled";
        params.course.value.scheduledStart = finalDateTime.value.toISOString();
    }

    function cancelPublicationSchedule() {
        params.course.value.status = params.originalCourse.value?.status ?? "draft";
        params.enabledModal.value = null;
        scheduleError.value = null;
    }

    return {
        confirmPublicationSchedule,
        cancelPublicationSchedule,

        formattedPublishTime,
        finalDateTime,

        dateOptions,
        selectedDateIso,
        selectedTime,
        isCustomDateEnabled,
        toggleCustomDate,
        customDateIso,
        minCustomDateIso,
        maxCustomDateIso,
        scheduleError,
    };
}

