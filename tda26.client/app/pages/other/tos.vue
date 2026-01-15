<script setup lang="ts">
    definePageMeta({
        layout: "normal-page-layout"
    });

    // SEO
    useSeo({
        title: "Podmínky používání",
        description: "Přečti si podmínky používání platformy Think Different Academy. Důležité informace o pravidlech a podmínkách pro uživatele.",
        noindex: true
    });
</script>

<template>
    <div :class="$style.container">
        <h1 :class="[$style.nadpis, 'text-gradient']">Podmínky používání</h1>
        <p :class="$style.podnapis">Vítej na platformě Think different Academy. Prosím, přečti si pečlivě následující podmínky používání.</p>

        <section :class="$style.section">
            <h2>1. Obecná ustanovení</h2>
            <p>Tyto podmínky používání (dále jen "Podmínky") upravují přístup a používání vzdělávací platformy Think different Academy (dále jen "Platforma"). Používáním této Platformy souhlasíš s těmito Podmínkami v plném rozsahu.</p>
        </section>

        <section :class="$style.section">
            <h2>2. Registrace a uživatelský účet</h2>
            <p>Pro přístup k některým funkcím Platformy je nutná registrace. Při registraci se zavazuješ:</p>
            <ul>
                <li>Poskytovat pravdivé a aktuální údaje</li>
                <li>Chránit své přihlašovací údaje před zneužitím</li>
                <li>Neposkytovat přístup k účtu třetím stranám</li>
                <li>Okamžitě nás informovat o jakémkoli neoprávněném použití tvého účtu</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>3. Používání obsahu</h2>
            <p>Veškerý obsah dostupný na Platformě, včetně kurzů, materiálů, videí a textů, je chráněn autorskými právy. Uživatelé se zavazují:</p>
            <ul>
                <li>Používat obsah pouze pro osobní vzdělávací účely</li>
                <li>Nesdílet přístupové údaje k placeným kurzům</li>
                <li>Nestahovat ani nešířit obsah bez souhlasu</li>
                <li>Respektovat práva lektorů a autorů obsahu</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>4. Placené služby</h2>
            <p>Některé kurzy a služby mohou být zpoplatněny. V případě placených služeb platí:</p>
            <ul>
                <li>Ceny jsou uvedeny vždy v českých korunách (CZK)</li>
                <li>Platba je splatná předem prostřednictvím dostupných platebních metod</li>
                <li>Po úspěšné platbě získáš přístup k zakoupenému obsahu</li>
                <li>Vrácení peněz je možné pouze v souladu s reklamačním řádem</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>5. Pravidla chování</h2>
            <p>Při používání Platformy se uživatelé zavazují:</p>
            <ul>
                <li>Dodržovat slušné chování vůči ostatním uživatelům a lektorům</li>
                <li>Nepublikovat urážlivý, diskriminační nebo nezákonný obsah</li>
                <li>Nepoužívat Platformu pro komerční účely bez souhlasu</li>
                <li>Neobtěžovat ostatní uživatele spamem nebo nevyžádanými zprávami</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>6. Omezení odpovědnosti</h2>
            <p>Think different Academy nenese odpovědnost za:</p>
            <ul>
                <li>Škody vzniklé nesprávným použitím Platformy uživatelem</li>
                <li>Dočasnou nedostupnost služeb z důvodu údržby nebo technických problémů</li>
                <li>Ztrátu dat způsobenou technickou poruchou</li>
                <li>Obsah vytvořený uživateli</li>
            </ul>
        </section>

        <section :class="$style.section">
            <h2>7. Změny podmínek</h2>
            <p>O významných změnách budeme uživatele informovat prostřednictvím emailu nebo oznámení na Platformě. Pokračováním v používání Platformy po změně Podmínek vyjadřuješ souhlas s aktualizovanou verzí.</p>
        </section>

        <section :class="$style.section">
            <h2>8. Ukončení přístupu</h2>
            <p>Vyhrazujeme si právo omezit nebo ukončit přístup k Platformě uživatelům, kteří porušují tyto Podmínky nebo se dopouštějí nezákonného jednání.</p>
        </section>

        <section :class="$style.section">
            <h2>9. Kontaktní informace</h2>
            <p>V případě dotazů ohledně těchto Podmínek nás prosím kontaktuj prostřednictvím kontaktního formuláře na našich webových stránkách.</p>
        </section>

        <section :class="$style.section">
            <h2>10. Závěrečná ustanovení</h2>
            <p>Tyto Podmínky se řídí právním řádem České republiky. Případné spory budou řešeny příslušnými soudy České republiky.</p>
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
}
</style>