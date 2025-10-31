<script setup lang="ts">
    definePageMeta({
        alias: "/privacy/cookies",
    })
</script>

<template>
    <div :class="$style.texts">
        <p>Naše webová aplikace používá soubory cookies, aby vám poskytla co nejlepší uživatelský zážitek. Cookies jsou malé textové soubory, které jsou ukládány ve vašem zařízení při návštěvě našich stránek. Pomáhají nám zapamatovat si vaše preference, zlepšit výkon našich služeb a přizpůsobit obsah vašim potřebám.</p>

        <h2>Jaké cookies používáme?</h2>
        <ul>
            <li><strong>Nezbytné cookies:</strong> Tyto cookies jsou nezbytné pro správnou funkci našich webových stránek a umožňují vám přístup k základním funkcím, jako je přihlášení do vaší aplikace.</li>
            <li><strong>Funkční cookies:</strong> Tyto cookies umožňují našim stránkám zapamatovat si vaše preference a přizpůsobit funkce, například jazykové nastavení.</li>
        </ul>

        <h2>Jak kontrolovat cookies?</h2>
        <p>Pokud nechcete, aby naše stránka používala cookies, můžete je ve svém prohlížeči zakázat. Upozorňujeme však, že některé funkce našich webových stránek nemusí být plně funkční, pokud cookies deaktivujete.</p>

        <p>Pokud budete pokračovat v používání našich stránek bez změny nastavení cookies, souhlasíte s jejich použitím.</p>
    </div>
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
