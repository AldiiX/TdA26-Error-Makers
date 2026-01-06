<script setup lang="ts">
    definePageMeta({
        layout: "normal-page-layout"
    });

    // SEO
    useSeo({
        title: "O nás",
        description: "Seznamte se s týmem Error Makers za Think Different Academy. Jsme tým studentů ze Střední školy EDUCHEM, kteří vytvořili tuto platformu pro Tour de App 2026.",
        keywords: "o nás, tým, vývojáři, Error Makers, Tour de App, EDUCHEM"
    });

    interface TeamMember {
        name: string;
        github: string;
        role: string;
        avatar: string;
        website: string;
    }

    const teamMembers: TeamMember[] = [
        {
            name: "Stanislav Škudrna",
            github: "AldiiX",
            role: "Fullstack · DevOps",
            avatar: "https://cloud0.emsio.cz/public/avatars/6f17f5a3-8d81-4f60-9260-ee69fb7a52d9.png",
            website: "https://stanislavskudrna.cz"
        },
        {
            name: "Serhii Yavorskyi",
            github: "WezeAnonym",
            role: "Fullstack · UI/UX",
            avatar: "https://cloud0.emsio.cz/public/avatars/38d32c1d-f592-4dd1-8238-3ac14fb7952e.png",
            website: "https://serhii.cz"
        },
        {
            name: "Jakub Sokol",
            github: "Jakooob",
            role: "Fullstack · Architecture",
            avatar: "https://cloud0.emsio.cz/public/avatars/11c27d7cf633d7058554aba6b6941caa.webp",
            website: "https://jakubsokol.cz"
        }
    ];

    interface Technology {
        name: string;
        category: string;
        icon: string;
    }

    const technologies: Technology[] = [
        { name: "Nuxt.js", category: "Frontend", icon: "nuxt" },
        { name: "Vue.js", category: "Frontend", icon: "vue" },
        { name: "SASS", category: "Frontend", icon: "sass" },
        { name: ".NET (C#)", category: "Backend", icon: "dotnet" },
        { name: "MySQL", category: "Databáze", icon: "mysql" },
        { name: "Redis", category: "Databáze", icon: "redis" },
        { name: "MinIO", category: "Objektová databáze", icon: "minio" }
    ];
</script>

<template>
    <Head>
        <Title>O nás • Think different Academy</Title>
    </Head>

    <div :class="$style.container">
        <h1 :class="[$style.nadpis, 'text-gradient']">O nás</h1>
        <p :class="$style.podnapis">Jsme tým Error Makers a vyvíjíme moderní vzdělávací platformu pro soutěž Tour de App 2026.</p>

        <section :class="$style.section">
            <h2>Náš příběh</h2>
            <p>Think different Academy vznikla jako projekt pro prestižní soutěž <strong>Tour de App 2026</strong>. Naším cílem je vytvořit platformu, která revolucionizuje způsob, jakým se lidé učí online. Věříme, že vzdělávání by mělo být interaktivní, přístupné a především zábavné.</p>
            <p>Spojili jsme moderní technologie s uživatelsky přívětivým designem, abychom vytvořili prostředí, kde každý najde kurzy odpovídající jeho potřebám a zájmům.</p>
        </section>

        <section :class="$style.section">
            <h2>Tým Error Makers</h2>
            <p>Za projektem stojí tým tří nadšených vývojářů ze <a href="https://educhem.cz" target="_blank" rel="noopener noreferrer">Střední školy EDUCHEM, a.s.</a></p>
            
            <div :class="$style.teamGrid">
                <div 
                    v-for="member in teamMembers" 
                    :key="member.github"
                    :class="$style.teamCard"
                >
                    <div :class="$style.avatarWrapper">
                        <img :src="member.avatar" :alt="member.name" :class="$style.avatar" />
                    </div>
                    <h3 :class="$style.memberName">{{ member.name }}</h3>
                    <p :class="$style.memberRole">{{ member.role }}</p>
                    <div :class="$style.memberLinks">
                        <a :href="member.website" target="_blank" rel="noopener noreferrer" :class="$style.link">
                            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                                <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
                            </svg>
                        </a>
                        <a :href="`https://github.com/${member.github}`" target="_blank" rel="noopener noreferrer" :class="$style.link">
                            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/>
                            </svg>
                        </a>
                    </div>
                </div>
            </div>
        </section>

        <section :class="$style.section">
            <h2>Technologie</h2>
            <p>Pro vývoj platformy využíváme moderní technologie, které zajišťují vysoký výkon, bezpečnost a skvělou uživatelskou zkušenost.</p>
            
            <div :class="$style.techGrid">
                <div 
                    v-for="tech in technologies" 
                    :key="tech.name"
                    :class="$style.techCard"
                >
                    <div :class="$style.techCategory">{{ tech.category }}</div>
                    <div :class="$style.techName">{{ tech.name }}</div>
                </div>
            </div>
        </section>

        <section :class="$style.section">
            <h2>Tour de App 2026</h2>
            <p>Naše platforma vznikla jako soutěžní projekt do <strong>Tour de App 2026</strong> – prestižní soutěže pro mladé vývojáře. Tour de App je unikátní příležitost ukázat své schopnosti, naučit se nové technologie a porovnat se s nejlepšími týmy z celé republiky.</p>
            <p>Soutěž se skládá z několika kol, kde týmy postupně vytvářejí funkční webovou aplikaci podle zadání. Hodnotí se nejen technická kvalita, ale i kreativita, uživatelská přívětivost a celkové zpracování projektu.</p>
        </section>

        <section :class="$style.section">
            <h2>Naše vize</h2>
            <p>Věříme, že vzdělávání by mělo být dostupné každému, bez ohledu na věk, místo nebo finanční situaci. Chceme vytvořit prostor, kde se mohou lidé učit novým dovednostem, rozvíjet své schopnosti a dosahovat svých cílů.</p>
            <p>Naše platforma kombinuje kvalitní obsah od zkušených lektorů s moderními technologiemi a intuitivním designem. Vytváříme místo, kde se učení stává radostí, ne povinností.</p>
        </section>

        <section :class="[$style.section, $style.contactCta]">
            <h2>Pojďme to změnit společně</h2>
            <p>Máte nápad na kurz? Chcete se stát lektorem? Nebo máte zpětnou vazbu k naší platformě? Neváhejte nás kontaktovat!</p>
        </section>
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

@keyframes fadeIn {
    from {
        opacity: 0;
    }
    to {
        opacity: 1;
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

.section {
    margin-top: 48px;
    animation: slideUp 0.6s ease-out backwards;
    
    &:nth-child(3) { animation-delay: 0.1s; }
    &:nth-child(4) { animation-delay: 0.15s; }
    &:nth-child(5) { animation-delay: 0.2s; }
    &:nth-child(6) { animation-delay: 0.25s; }
    &:nth-child(7) { animation-delay: 0.3s; }
    &:nth-child(8) { animation-delay: 0.35s; }

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

        strong {
            color: var(--accent-color-primary);
            font-weight: 600;
        }

        a {
            color: var(--accent-color-primary);
            text-decoration: none;
            transition: opacity 0.3s ease;

            &:hover {
                opacity: 0.8;
                text-decoration: underline;
            }
        }
    }
}

.teamGrid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 24px;
    margin-top: 32px;
}

.teamCard {
    background: var(--background-secondary);
    border-radius: 16px;
    padding: 32px;
    text-align: center;
    transition: all 0.3s ease;
    border: 1px solid transparent;
    animation: fadeIn 0.6s ease-out backwards;

    &:nth-child(1) { animation-delay: 0.2s; }
    &:nth-child(2) { animation-delay: 0.3s; }
    &:nth-child(3) { animation-delay: 0.4s; }

    &:hover {
        transform: translateY(-8px);
        border-color: var(--accent-color-primary);
        box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
    }
}

.avatarWrapper {
    margin-bottom: 16px;
}

.avatar {
    width: 96px;
    height: 96px;
    border-radius: 50%;
    object-fit: cover;
    border: 3px solid var(--accent-color-primary);
}

.memberName {
    margin: 0 0 8px 0;
    font-size: 20px;
    font-weight: 600;
    color: var(--text-color-primary);
}

.memberRole {
    margin: 0 0 16px 0;
    font-size: 14px;
    color: var(--text-color-secondary);
    opacity: 0.7;
}

.memberLinks {
    display: flex;
    justify-content: center;
    gap: 12px;
}

.link {
    width: 36px;
    height: 36px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: var(--accent-color-primary-transparent-02);
    border-radius: 8px;
    color: var(--accent-color-primary);
    transition: all 0.3s ease;

    &:hover {
        background: var(--accent-color-primary);
        color: white;
        transform: scale(1.1);
    }
}

.techGrid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 16px;
    margin-top: 24px;

    >div {
        font-family: Dosis, 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }
}

.techCard {
    background: var(--background-secondary);
    border-radius: 12px;
    padding: 20px;
    border: 1px solid transparent;
    transition: all 0.3s ease;
    animation: fadeIn 0.6s ease-out backwards;

    &:nth-child(1) { animation-delay: 0.2s; }
    &:nth-child(2) { animation-delay: 0.25s; }
    &:nth-child(3) { animation-delay: 0.3s; }
    &:nth-child(4) { animation-delay: 0.35s; }
    &:nth-child(5) { animation-delay: 0.4s; }
    &:nth-child(6) { animation-delay: 0.45s; }
    &:nth-child(7) { animation-delay: 0.5s; }

    &:hover {
        border-color: var(--accent-color-primary);
        transform: translateY(-4px);
    }
}

.techCategory {
    font-size: 12px;
    text-transform: uppercase;
    color: var(--text-color-secondary);
    opacity: 0.5;
    margin-bottom: 8px;
    font-weight: 600;
    letter-spacing: 1px;
}

.techName {
    font-size: 18px;
    font-weight: 600;
    color: var(--text-color-primary);
}

.contactCta {
    margin-top: 64px;
    padding: 32px;
    background: linear-gradient(135deg, var(--accent-color-primary-transparent-02), var(--accent-color-secondary-transparent-02));
    border-radius: 16px;
    text-align: center;

    h2 {
        margin-bottom: 12px;
    }

    p {
        font-size: 18px;
    }
}

@media screen and (max-width: 768px) {
    .nadpis {
        font-size: 48px;
    }

    .teamGrid {
        grid-template-columns: 1fr;
    }

    .techGrid {
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    }
}
</style>