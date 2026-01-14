<script setup lang="ts">
    definePageMeta({
        layout: "normal-page-layout"
    });

    // SEO
    useSeo({
        title: "GDPR - Ochrana osobních údajů",
        description: "Informace o ochraně osobních údajů na Think Different Academy v souladu s GDPR. Zjistěte, jak zpracováváme a chráníme vaše osobní údaje.",
        noindex: true
    });
</script>

<template>
    <div :class="$style.container">
        <h1 :class="[$style.nadpis, 'text-gradient']">Ochrana osobních údajů (GDPR)</h1>
        <p :class="$style.podnapis">Think different Academy respektuje tvoje soukromí a zavazuje se chránit tvoje osobní údaje v souladu s Nařízením Evropského parlamentu a Rady (EU) 2016/679 o ochraně osobních údajů (GDPR).</p>

        <section :class="$style.section">
            <h2>1. Správce osobních údajů</h2>
            <p>Správcem tvých osobních údajů je Think different Academy. V případě dotazů týkajících se zpracování osobních údajů nás můžeš kontaktovat prostřednictvím kontaktního formuláře na našich webových stránkách.</p>
        </section>

        <section :class="$style.section">
            <h2>2. Jaké osobní údaje zpracováváme</h2>
            <p>V rámci provozu naší vzdělávací platformy můžeme zpracovávat následující kategorie osobních údajů:</p>
            <ul>
                <li>Identifikační údaje (jméno, příjmení)</li>
                <li>Kontaktní údaje (emailová adresa, telefonní číslo)</li>
                <li>Údaje o používání platformy (historie kurzů, pokrok ve vzdělávání)</li>
                <li>Platební údaje (pro zpracování plateb za kurzy)</li>
                <li>Technické údaje (IP adresa, informace o zařízení a prohlížeči)</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>3. Účel zpracování osobních údajů</h2>
            <p>Tvoje osobní údaje zpracováváme pro následující účely:</p>
            <ul>
                <li>Poskytování přístupu k vzdělávací platformě a kurzům</li>
                <li>Vytvoření a správa uživatelského účtu</li>
                <li>Zpracování plateb a vedení účetnictví</li>
                <li>Komunikace s uživateli ohledně kurzů a služeb</li>
                <li>Zlepšování kvality našich služeb a obsahu</li>
                <li>Plnění právních povinností</li>
                <li>Ochrana našich oprávněných zájmů</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>4. Právní základ zpracování</h2>
            <p>Osobní údaje zpracováváme na základě:</p>
            <ul>
                <li>Tvého souhlasu (zejména při registraci)</li>
                <li>Plnění smlouvy o poskytování vzdělávacích služeb</li>
                <li>Plnění právních povinností</li>
                <li>Oprávněného zájmu správce (zlepšování služeb, bezpečnost platformy)</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>5. Doba uchovávání osobních údajů</h2>
            <p>Osobní údaje uchováváme pouze po dobu nezbytně nutnou pro naplnění účelu zpracování:</p>
            <ul>
                <li>Údaje uživatelského účtu po dobu trvání účtu a po jeho zrušení ještě po dobu stanovenou právními předpisy</li>
                <li>Údaje potřebné pro splnění zákonných povinností po dobu stanovenou příslušnými právními předpisy (např. účetní doklady 10 let)</li>
                <li>Marketingové údaje do odvolání souhlasu nebo vznesení námitky</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>6. Předávání osobních údajů</h2>
            <p>Tvoje osobní údaje můžeme předávat:</p>
            <ul>
                <li>Poskytovatelům platebních služeb (pro zpracování plateb)</li>
                <li>Poskytovatelům hostingových služeb</li>
                <li>Poskytovatelům analytických nástrojů</li>
                <li>Orgánům veřejné moci na základě právních předpisů</li>
            </ul>
            <p>Všichni zpracovatelé jsou pečlivě vybráni a zavázáni k ochraně tvých osobních údajů.</p>
        </section>

        <section :class="$style.section">
            <h2>7. Tvoje práva</h2>
            <p>V souvislosti se zpracováním tvých osobních údajů máš následující práva:</p>
            <ul>
                <li><strong>Právo na přístup:</strong> Máš právo získat informace o tom, jaké osobní údaje o tobě zpracováváme</li>
                <li><strong>Právo na opravu:</strong> Máš právo požadovat opravu nepřesných nebo neúplných údajů</li>
                <li><strong>Právo na výmaz:</strong> Za určitých podmínek můžeš požadovat výmaz tvých osobních údajů</li>
                <li><strong>Právo na omezení zpracování:</strong> Můžeš požádat o omezení zpracování tvých údajů</li>
                <li><strong>Právo na přenositelnost:</strong> Máš právo získat své údaje ve strukturovaném, běžně používaném formátu</li>
                <li><strong>Právo vznést námitku:</strong> Můžeš vznést námitku proti zpracování tvých údajů</li>
                <li><strong>Právo odvolat souhlas:</strong> Pokud je zpracování založeno na souhlasu, máš právo jej kdykoli odvolat</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>8. Bezpečnost osobních údajů</h2>
            <p>Zavázali jsme se chránit tvoje osobní údaje pomocí vhodných technických a organizačních opatření, včetně šifrování dat, pravidelných bezpečnostních auditů a omezení přístupu k osobním údajům pouze na oprávněné osoby.</p>
        </section>

        <section :class="$style.section">
            <h2>9. Cookies</h2>
            <p>Naše webová stránka používá cookies pro zajištění správné funkčnosti a zlepšení uživatelského zážitku. Podrobnosti o používání cookies najdete v naší <a href="/other/cookies">Zásadách používání cookies</a>.</p>
        </section>

        <section :class="$style.section">
            <h2>10. Změny těchto zásad</h2>
            <p>Tyto zásady ochrany osobních údajů můžeme čas od času aktualizovat. O významných změnách tě budeme informovat prostřednictvím našich webových stránek nebo e-mailem.</p>
        </section>

        <section :class="$style.section">
            <h2>11. Kontakt a podání stížnosti</h2>
            <p>Pro uplatnění tvých práv nebo pro jakékoli dotazy týkající se zpracování osobních údajů nás prosím kontaktuj prostřednictvím kontaktního formuláře na našich webových stránkách.</p>
            <p :class="$style.lastUpdate">Poslední aktualizace: Listopad 2025</p>
        </section>
    </div>
</template>

<style module lang="scss">
@use "../../app" as app;

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

.container {
    animation: slideUp 0.6s ease-out;
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
    &:nth-child(11) { animation-delay: 0.5s; }
    &:nth-child(12) { animation-delay: 0.55s; }
    &:nth-child(13) { animation-delay: 0.6s; }

    h2 {
        font-size: 32px;
        margin: 0 0 16px 0;
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

    a {
        color: var(--color-primary, #007bff);
        text-decoration: none;
        opacity: 0.7;
        
        &:hover {
            text-decoration: underline;
        }
    }
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