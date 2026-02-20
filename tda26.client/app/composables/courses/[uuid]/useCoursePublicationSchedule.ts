import type {Course, CourseStatus, TimeOption} from "#shared/types";
import type {CourseDetailModal} from "~/composables/courses/[uuid]/courseDetailTypes";
import { type Ref } from "vue";

function getLocalDateTimeString(): string {
    const now = new Date()
    now.setMinutes(now.getMinutes() - now.getTimezoneOffset())
    return now.toISOString().slice(0, 16)
}



export default function(params: {
    enabledModal: Ref<CourseDetailModal>;
    course: Ref<Course>;
    updateError: Ref<string | null>;
    deleteError: Ref<string | null>;
    originalCourse: Ref<Course | null>;
}) {
    const customDateTime = ref<string>("");
    const selectedTimeOption = ref<number | "custom">(5);
    const maxCustomDatetime = "2100-01-01T12:00"
    const minCustomDatetime = computed(() => getLocalDateTimeString())
    const timeOptions: TimeOption[] = [
        { label: "Za 5 minut", values: 5 },
        { label: "Za 15 minut", values: 15 },
        { label: "Za 30 minut", values: 30 },
        { label: "Za 1 hodinu", values: 60 },
        { label: "Za 12 hodin", values: 720 },
        { label: "Vlastní datum a čas", values: "custom" }
    ];


    const finalDateTime = computed<Date | null>(() => {
        if (selectedTimeOption.value === "custom") {
            if (!customDateTime.value) return null;

            const parsed = new Date(customDateTime.value);


            return isNaN(parsed.getTime()) ? null : parsed;
        }

        const now = new Date();
        return new Date(now.getTime() + Number(selectedTimeOption.value) * 60000);
    });

    watch(params.course, (newValue) => {
        if (newValue.status === "scheduled") {
            params.enabledModal.value = "schedulePublication";
        }
    });

    watch(selectedTimeOption, (val) => {
        if (val === "custom") {
            const now = new Date()
            now.setMinutes(now.getMinutes() + 5)
            now.setSeconds(0)

            const local = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
            customDateTime.value = local.toISOString().slice(0, 16)
        }
    })

    async function confirmPublicationSchedule() {
        if (!finalDateTime.value || !params.course.value) return;

        await $fetch(`/api/v1/courses/${params.course.value.uuid}/status`, {
            method: "PUT",
            body: {
                status: "scheduled",
                scheduledStart: finalDateTime.value.toISOString()
            }
        });

        params.enabledModal.value = null;
        params.course.value.status = "scheduled";
    }

    function cancelPublicationSchedule() {
        params.course.value.status = params.originalCourse.value?.status ?? "draft";
        params.enabledModal.value = null;
    }

    const formattedPublishTime = computed(() =>
        formatDateTime(finalDateTime.value)
    );

    watch(params.enabledModal, (val) => {
        if (val === null) return;

        params.updateError.value = null;
        params.deleteError.value = null;
    });

    return {
        confirmPublicationSchedule,
        cancelPublicationSchedule,
        formattedPublishTime,
        selectedTimeOption,
        timeOptions,
        customDateTime,
        maxCustomDatetime,
        minCustomDatetime,
        finalDateTime,
    }
}