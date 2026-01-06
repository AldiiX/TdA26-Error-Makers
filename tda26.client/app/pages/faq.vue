<script setup lang="ts">
    import { ref } from 'vue';

    definePageMeta({
        layout: "normal-page-layout"
    });

    // SEO
    useSeo({
        title: "Často kladené otázky (FAQ)",
        description: "Máte otázky ohledně Think Different Academy? Najděte odpovědi na nejčastěji kladené otázky o registraci, kurzech, ceníku a dalších funkcích platformy.",
        keywords: "FAQ, často kladené otázky, help, nápověda, podpora"
    });

    interface FaqItem {
        question: string;
        answer: string;
    }

    const openIndex = ref<number | null>(null);

    const toggleFaq = (index: number) => {
        openIndex.value = openIndex.value === index ? null : index;
    };

    const faqItems: FaqItem[] = [
        {
            question: "Co je Think different Academy?",
            answer: "Think different Academy je moderní vzdělávací platforma vytvořená pro soutěž Tour de App 2026. Naším cílem je revolucionizovat způsob, jakým se lidé učí online. Poskytujeme interaktivní kurzy, které kombinují teoretické znalosti s praktickými dovednostmi."
        },
        {
            question: "Jak se mohu zaregistrovat?",
            answer: "Registrace je velmi jednoduchá! Stačí kliknout na tlačítko 'Registrace' v horní části stránky, vyplnit své údaje a ověřit e-mailovou adresu. Po dokončení registrace získáte okamžitý přístup k bezplatným kurzům a budete moci procházet celou nabídku."
        },
        {
            question: "Jsou kurzy zdarma?",
            answer: "Nabízíme jak bezplatné, tak prémiové kurzy. Všichni uživatelé mají přístup k vybraným základním kurzům zdarma. Pokročilejší a specializované kurzy jsou dostupné jako placené, což nám umožňuje neustále vylepšovat kvalitu obsahu a přidávat nové materiály."
        },
        {
            question: "Kdo jsou lektoři?",
            answer: "Naši lektoři jsou zkušení odborníci a nadšenci ve svých oborech. Každý lektor je pečlivě vybrán na základě svých znalostí, pedagogických schopností a praktických zkušeností. Jejich profily a specializace můžete prozkoumat na stránce Lektoři."
        },
        {
            question: "Jak dlouho trvá dokončení kurzu?",
            answer: "Doba trvání kurzu závisí na jeho složitosti a rozsahu. U každého kurzu najdete odhadovanou dobu dokončení. Výhodou naší platformy je flexibilita – můžete se učit vlastním tempem a kdykoliv se ke kurzu vrátit."
        },
        {
            question: "Získám certifikát po dokončení kurzu?",
            answer: "Ano! Po úspěšném dokončení kurzu obdržíte digitální certifikát, který můžete sdílet na sociálních sítích nebo přidat do svého životopisu. Certifikát obsahuje informace o kurzu, vaše jméno a datum dokončení."
        },
        {
            question: "Mohu se učit z mobilního zařízení?",
            answer: "Samozřejmě! Naše platforma je plně responzivní a optimalizovaná pro všechna zařízení – počítače, tablety i mobilní telefony. Můžete se učit kdekoli a kdykoli vám to vyhovuje."
        },
        {
            question: "Co když mi kurz nevyhovuje?",
            answer: "Chceme, abyste byli spokojeni! V případě placených kurzů nabízíme 14denní záruku vrácení peněz. Pokud z jakéhokoli důvodu nejste spokojeni s kurzem, kontaktujte nás a vrátíme vám platbu."
        },
        {
            question: "Jak mohu kontaktovat podporu?",
            answer: "Náš tým podpory je vám k dispozici prostřednictvím kontaktního formuláře na webu. Snažíme se odpovědět na všechny dotazy do 24 hodin. Pro rychlejší odpovědi se můžete také podívat do této sekce FAQ, kde řešíme nejčastější dotazy."
        },
        {
            question: "Mohu se stát lektorem?",
            answer: "Určitě! Vždy hledáme talentované lektory, kteří by chtěli sdílet své znalosti. Pokud máte zkušenosti v určité oblasti a chcete vytvářet kurzy, kontaktujte nás prostřednictvím sekce 'Staň se lektorem' nebo nám napište na náš e-mail."
        }
    ];
</script>

<template>
    <Head>
        <Title>FAQ • Think different Academy</Title>
    </Head>

    <div :class="$style.container">
        <h1 :class="[$style.nadpis, 'text-gradient']">Často kladené otázky</h1>
        <p :class="$style.podnapis">Máte dotazy? Najděte odpovědi na nejčastější otázky ohledně naší platformy, kurzů a služeb.</p>

        <div :class="$style.faqList">
            <div 
                v-for="(item, index) in faqItems" 
                :key="index"
                :class="[$style.faqItem, { [$style.open]: openIndex === index }]"
                @click="toggleFaq(index)"
            >
                <div :class="$style.question">
                    <h3>{{ item.question }}</h3>
                    <div :class="$style.icon">
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path d="M6 9L12 15L18 9" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                        </svg>
                    </div>
                </div>
                <div :class="$style.answer">
                    <p>{{ item.answer }}</p>
                </div>
            </div>
        </div>

        <div :class="$style.contactSection">
            <h2>Nenašli jste odpověď?</h2>
            <p>Pokud jste nenašli odpověď na svou otázku, neváhejte nás kontaktovat. Rádi vám pomůžeme!</p>
        </div>
    </div>
</template>

<style module lang="scss">
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
    opacity: 0.7;
    animation: slideUp 0.8s ease-out 0.1s backwards;
}

.faqList {
    margin-top: 48px;
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.faqItem {
    background: var(--background-secondary);
    border-radius: 12px;
    padding: 24px;
    cursor: pointer;
    transition: all 0.3s ease;
    border: 1px solid transparent;
    animation: slideUp 0.6s ease-out backwards;

    &:nth-child(1) { animation-delay: 0.2s; }
    &:nth-child(2) { animation-delay: 0.25s; }
    &:nth-child(3) { animation-delay: 0.3s; }
    &:nth-child(4) { animation-delay: 0.35s; }
    &:nth-child(5) { animation-delay: 0.4s; }
    &:nth-child(6) { animation-delay: 0.45s; }
    &:nth-child(7) { animation-delay: 0.5s; }
    &:nth-child(8) { animation-delay: 0.55s; }
    &:nth-child(9) { animation-delay: 0.6s; }
    &:nth-child(10) { animation-delay: 0.65s; }

    &:hover {
        border-color: var(--accent-color-primary);
        transform: translateY(-2px);
    }

    &.open {
        .icon svg {
            transform: rotate(180deg);
        }

        .answer {
            max-height: 500px;
            opacity: 1;
            margin-top: 16px;
        }
    }
}

.question {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 16px;

    h3 {
        margin: 0;
        font-size: 20px;
        color: var(--text-color-primary);
        font-weight: 600;
    }
}

.icon {
    flex-shrink: 0;
    width: 24px;
    height: 24px;
    color: var(--accent-color-primary);

    svg {
        transition: transform 0.3s ease;
    }
}

.answer {
    max-height: 0;
    opacity: 0;
    overflow: hidden;
    transition: all 0.3s ease;

    p {
        margin: 0;
        font-size: 16px;
        line-height: 1.6;
        color: var(--text-color-secondary);
        opacity: 0.7;
    }
}

.contactSection {
    margin-top: 64px;
    padding: 32px;
    background: linear-gradient(135deg, var(--accent-color-primary-transparent-02), var(--accent-color-secondary-transparent-02));
    border-radius: 16px;
    text-align: center;
    animation: slideUp 0.6s ease-out 0.7s backwards;

    h2 {
        margin: 0 0 12px 0;
        font-size: 28px;
        color: var(--text-color-primary);
    }

    p {
        margin: 0;
        font-size: 18px;
        color: var(--text-color-secondary);
        opacity: 0.7;
    }
}
</style>