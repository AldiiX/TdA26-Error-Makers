<script setup lang="ts">
    import ColoredBGLayout from "@/layouts/ColoredBGLayout.vue";
    definePageMeta({
        alias: ["/privacy/gdpr"]
    })
</script>

<template>
    <ColoredBGLayout text="GDPR">
        <div :class="$style.texts">
            <p>Aplikace Piškvorky respektuje zásady Obecného nařízení o ochraně osobních údajů (GDPR) a dbá na ochranu soukromí svých uživatelů.</p>
            <p>Při přihlašování aplikace shromažďuje pouze nezbytné údaje, jako je uživatelské jméno a heslo, které slouží výhradně pro účely autentifikace uživatele. Tyto údaje jsou bezpečně uloženy a zpracovávány v souladu s GDPR a nejsou sdíleny s žádnými třetími stranami.</p>
            <p>Aplikace neumožňuje uživatelům zobrazení nebo úpravu jejich údajů prostřednictvím rozhraní. Uživatelé nemají přístup k jejich údajům v aplikaci, ale mohou požádat o odstranění svého účtu nebo údajů prostřednictvím kontaktování správce aplikace. V takovém případě budou jejich údaje vymazány v souladu s požadavky GDPR.</p>
            <p>Tento přístup zajistí, že aplikace Piškvorky splňuje požadavky GDPR a chrání soukromí svých uživatelů.</p>
        </div>
    </ColoredBGLayout>
</template>

<style module lang="scss">
.sections{
    .mainsection{

        .texts{
            width: 80%;
            margin: 64px auto;
            margin-bottom: calc(64px + 40px);
            display: flex;
            flex-direction: column;
            gap: 16px;

            p {
                text-align: justify;
                margin: 0;
                font-size: 20px;
            }
        }
    }
}

@media screen and (min-width: 600px) and (max-width: 960px) {
    .sections .mainsection .texts p {
        font-size: 16px;
    }
}

@media screen and (max-width: 600px) {
    .sections .mainsection .texts p {
        font-size: 16px;
    }
}

</style>
