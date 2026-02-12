<script setup lang="ts">
    definePageMeta({
        layout: "normal-page-layout"
    });

    // SEO
    useSeo({
        title: "Zásady používání cookies",
        description: "Přečti si informace o tom, jak Think Different Academy používá cookies k zlepšení tvého zážitku z používání platformy.",
        noindex: true // Legal pages often noindex
    });
</script>

<template>
    <div :class="$style.container">
        <div :class="$style.headerSection">
            <h1 :class="[$style.nadpis, 'text-gradient']">Zásady používání cookies</h1>
            <div :class="$style.cookieIcon" />
        </div>
        <p :class="$style.podnapis">Vítej na platformě Think different Academy. Tato stránka vysvětluje, jak používáme cookies k zlepšení tvého zážitku z používání našich služeb.</p>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>1. Co jsou cookies?</h2>
            </div>
            <p>Cookies jsou malé textové soubory, které se ukládají do tvého zařízení při návštěvě webové stránky. Umožňují webu zapamatovat si tvoje preference a zlepšit tvůj uživatelský zážitek.</p>
            <p>Cookies mohou být:</p>
            <ul>
                <li><strong>Dočasné (session cookies)</strong> - jsou automaticky smazány po zavření prohlížeče</li>
                <li><strong>Trvalé (persistent cookies)</strong> - zůstávají uloženy po určitou dobu</li>
                <li><strong>Vlastní (first-party)</strong> - nastavené přímo naší platformou</li>
                <li><strong>Třetích stran (third-party)</strong> - nastavené externími službami</li>
            </ul>
        </section>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>2. Jaké cookies používáme?</h2>
            </div>
            <p>Na naší platformě používáme následující typy cookies:</p>
            
            <div :class="$style.cookieType">
                <h3>Nezbytné cookies</h3>
                <p>Tyto cookies jsou nezbytné pro správné fungování platformy. Bez nich by některé funkce nebyly dostupné.</p>
                <ul>
                    <li>Autentizační cookies pro přihlášení uživatelů</li>
                    <li>Cookies pro zabezpečení relace</li>
                    <li>Cookies pro uchování jazykových preferencí</li>
                </ul>
            </div>

            <div :class="$style.cookieType">
                <h3>Výkonnostní cookies</h3>
                <p>Pomáhají nám pochopit, jak návštěvníci používají naši platformu, což nám umožňuje ji vylepšovat.</p>
                <ul>
                    <li>Sledování počtu návštěv a zdrojů návštěvnosti</li>
                    <li>Měření výkonu stránek</li>
                    <li>Analýza chování uživatelů</li>
                </ul>
            </div>

            <div :class="$style.cookieType">
                <h3>Funkční cookies</h3>
                <p>Umožňují platformě zapamatovat si tvoje volby a poskytovat vylepšené funkce.</p>
                <ul>
                    <li>Uchování nastavení zobrazení (tmavý/světlý režim)</li>
                    <li>Preferované nastavení kurzů</li>
                    <li>Personalizované doporučení obsahu</li>
                </ul>
            </div>
        </section>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>3. Správa cookies</h2>
            </div>
            <p>Máš plnou kontrolu nad tím, jaké cookies akceptuješ. Cookies můžeš spravovat následujícími způsoby:</p>
            <ul>
                <li><strong>Nastavení prohlížeče</strong> - většina prohlížečů umožňuje blokovat nebo mazat cookies</li>
                <li><strong>Nástroje třetích stran</strong> - můžeš použít rozšíření prohlížeče pro správu cookies</li>
                <li><strong>Naše nastavení</strong> - v budoucnu budeme poskytovat nástroj pro správu preferencí cookies</li>
            </ul>
            <p>Upozorňujeme, že zablokování některých cookies může ovlivnit funkčnost platformy a tvůj uživatelský zážitek.</p>
        </section>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>4. Cookies třetích stran</h2>
            </div>
            <p>Používáme také služby třetích stran, které mohou nastavovat vlastní cookies:</p>
            <ul>
                <li><strong>Analytické nástroje</strong> - pro měření návštěvnosti a chování uživatelů</li>
                <li><strong>Platební brány</strong> - pro bezpečné zpracování plateb</li>
                <li><strong>Video platformy</strong> - pro přehrávání vzdělávacího obsahu</li>
            </ul>
            <p>Tyto služby mají vlastní zásady ochrany soukromí, které doporučujeme prostudovat.</p>
        </section>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>5. Doba uchování cookies</h2>
            </div>
            <p>Různé typy cookies uchovávame po různou dobu:</p>
            <ul>
                <li><strong>Session cookies</strong> - smazány po zavření prohlížeče</li>
                <li><strong>Autentizační cookies</strong> - až 30 dní nebo do odhlášení</li>
                <li><strong>Preferenční cookies</strong> - až 1 rok</li>
                <li><strong>Analytické cookies</strong> - až 2 roky</li>
            </ul>
        </section>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>6. Jak smazat cookies?</h2>
            </div>
            <p>Pokud si přeješ smazat cookies, můžeš tak učinit prostřednictvím nastavení tvého prohlížeče:</p>
            <ul>
                <li><strong>Google Chrome</strong> - Nastavení → Soukromí a zabezpečení → Cookies a další data webů</li>
                <li><strong>Mozilla Firefox</strong> - Možnosti → Soukromí a zabezpečení → Cookies a data stránek</li>
                <li><strong>Safari</strong> - Předvolby → Soukromí → Správa dat webových stránek</li>
                <li><strong>Microsoft Edge</strong> - Nastavení → Soukromí, vyhledávání a služby → Cookies</li>
            </ul>
        </section>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>7. Aktualizace těchto zásad</h2>
            </div>
            <p>Tyto zásady používání cookies můžeme čas od času aktualizovat, abychom odráželi změny v našich postupech nebo z právních důvodů. O významných změnách tě budeme informovat prostřednictvím oznámení na platformě nebo emailem.</p>
        </section>

        <section :class="$style.section">
            <div :class="$style.sectionHeader">
                <h2>8. Kontakt</h2>
            </div>
            <p>Pokud máš jakékoli dotazy ohledně našeho používání cookies, neváhej nás kontaktovat prostřednictvím kontaktního formuláře na našich webových stránkách.</p>
            <p :class="$style.lastUpdate">Poslední aktualizace: Listopad 2025</p>
        </section>
    </div>
</template>

<style module lang="scss">
@use "@/assets/variables" as app;

@keyframes slideUp {
    from {
        opacity: 0;
        transform: translateY(50px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

@keyframes float {
    0%, 100% {
        transform: translateY(0px) rotate(0deg);
    }
    25% {
        transform: translateY(-10px) rotate(5deg);
    }
    50% {
        transform: translateY(-5px) rotate(-5deg);
    }
    75% {
        transform: translateY(-15px) rotate(3deg);
    }
}

.container {
    animation: slideUp 0.6s ease-out;
}

.headerSection {
    display: flex;
    align-items: center;
    gap: 24px;
    flex-wrap: wrap;
}

.cookieIcon {
    mask-image: url('/icons/cookie.svg');
    mask-size: contain;
    mask-repeat: no-repeat;
    mask-position: center;
    background-color: var(--accent-color-secondary-theme);
    width: 80px;
    height: 80px;
    animation: slideUp 0.8s ease-out;
}

.nadpis {
    font-size: 64px;
    margin: 0;
    animation: slideUp 0.8s ease-out;
}

.podnapis {
    font-size: 20px;
    margin-top: 16px;
    max-width: 700px;
    color: var(--text-color-secondary);
    animation: slideUp 0.8s ease-out 0.1s backwards;
}

.section {
    margin-top: 40px;
    animation: slideUp 0.6s ease-out backwards;
    
    &:nth-child(3) { animation-delay: 0.1s; }
    &:nth-child(4) { animation-delay: 0.15s; }
    &:nth-child(5) { animation-delay: 0.2s; }
    &:nth-child(6) { animation-delay: 0.25s; }
    &:nth-child(7) { animation-delay: 0.3s; }
    &:nth-child(8) { animation-delay: 0.35s; }
    &:nth-child(9) { animation-delay: 0.4s; }
    &:nth-child(10) { animation-delay: 0.45s; }

    .sectionHeader {
        display: flex;
        align-items: center;
        gap: 16px;
        margin-bottom: 16px;
    }

    .sectionIcon {
        width: 32px;
        height: 32px;
        color: var(--color-primary);
        flex-shrink: 0;
    }

    h2 {
        font-size: 32px;
        margin: 0;
        color: var(--text-color-primary);
    }

    h3 {
        font-size: 24px;
        margin: 24px 0 12px 0;
        color: var(--text-color-primary);
    }

    p {
        font-size: 18px;
        line-height: 1.6;
        color: var(--text-color-secondary);
        margin: 12px 0;
        opacity: 0.7;
    }

    ul {
        margin: 12px 0;
        padding-left: 24px;

        li {
            font-size: 18px;
            line-height: 1.8;
            color: var(--text-color-secondary);
            margin: 8px 0;
            opacity: 0.7;

            strong {
                color: var(--text-color-primary);
            }
        }
    }
}

.cookieType {
    margin: 24px 0;
    padding: 20px;
    background: var(--background-color-secondary);
    border-radius: 12px;
    border-left: 4px solid var(--color-primary);
}

.lastUpdate {
    margin-top: 24px;
    font-style: italic;
    font-size: 16px;
    color: var(--color-gray);
}

@media screen and (min-width: app.$mobileBreakpoint) and (max-width: app.$tabletBreakpoint) {
    .nadpis {
        font-size: 52px;
    }
}

@media screen and (max-width: app.$mobileBreakpoint) {
    .nadpis {
        font-size: 40px;
    }

    .container {
        margin-top: -50px;
    }
    
    .cookieIcon {
        width: 60px;
        height: 60px;
    }
}
</style>