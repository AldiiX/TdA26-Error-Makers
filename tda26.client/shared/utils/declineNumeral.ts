type CzechForms =
    | string
    | {
    one: string;
    few?: string;
    many?: string;
};

export function formatCzechCount(count: number, forms: CzechForms): string {
    const absCount = Math.abs(count);

    // Pokud je jen jeden tvar (např. "zhlédnutí")
    if (typeof forms === "string") {
        return forms;
    }

    const lastTwo = absCount % 100;
    const lastOne = absCount % 10;

    // Výjimka 11–14
    if (lastTwo >= 11 && lastTwo <= 14) {
        return forms.many ?? forms.one;
    }

    if (lastOne === 1) {
        return forms.one;
    }

    if (lastOne >= 2 && lastOne <= 4) {
        return forms.few ?? forms.one;
    }

    return forms.many ?? forms.one;
}
