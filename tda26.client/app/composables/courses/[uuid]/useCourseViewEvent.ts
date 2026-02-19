import { onMounted } from "vue";
import type { gRecaptcha } from "#shared/types";

declare const grecaptcha: gRecaptcha;

export function useCourseViewEvent(uuid: string) {
    onMounted(async () => {
        if (!import.meta.client) return;

        // reCAPTCHA musi byt nactena, jinak skip
        if (typeof grecaptcha === "undefined") {
            console.warn("reCAPTCHA not loaded, skipping view event");
            return;
        }

        grecaptcha.ready(async () => {
            const captchaToken = await grecaptcha.execute(
                "6LfDQhksAAAAAEz_ujbJNian3-e-TfyKx8gzRaCL",
                { action: "submit" }
            );

            await fetch(`/api/v1/courses/${uuid}/view`, {
                method: "POST",
                body: JSON.stringify({ token: captchaToken }),
                headers: {
                    "Content-Type": "application/json"
                }
            });
        });
    });
}
