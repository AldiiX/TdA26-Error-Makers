import { useHead, useRoute } from '#imports'

export interface SeoOptions {
    title?: string
    description?: string
    image?: string
    keywords?: string
    type?: 'website' | 'article'
    noindex?: boolean
}

export const useSeo = (options: SeoOptions = {}) => {
    const route = useRoute()
    const baseUrl = 'https://ghrp-679926fd-aldiix-tda26-error-makers-app.emsio.cz'
    const siteName = 'Think Different Academy'
    
    const defaultDescription = 'Interaktivní vzdělávací platforma pro studenty a lektory. Objevte kurzy, kvízy a mnoho dalšího na Think Different Academy.'
    const defaultImage = `${baseUrl}/icons/logo_gradient.png`
    
    const pageTitle = options.title 
        ? `${options.title} • ${siteName}` 
        : siteName
    
    const description = options.description || defaultDescription
    const image = options.image || defaultImage
    const canonical = `${baseUrl}${route.path}`
    const type = options.type || 'website'
    
    useHead({
        title: pageTitle,
        meta: [
            { name: 'description', content: description },
            ...(options.keywords ? [{ name: 'keywords', content: options.keywords }] : []),
            { name: 'robots', content: options.noindex ? 'noindex, nofollow' : 'index, follow' },
            
            // Open Graph
            { property: 'og:type', content: type },
            { property: 'og:site_name', content: siteName },
            { property: 'og:title', content: pageTitle },
            { property: 'og:description', content: description },
            { property: 'og:url', content: canonical },
            { property: 'og:image', content: image },
            { property: 'og:image:width', content: '344' },
            { property: 'og:image:height', content: '376' },
            { property: 'og:locale', content: 'cs_CZ' },
            
            // Twitter Card
            { name: 'twitter:card', content: 'summary_large_image' },
            { name: 'twitter:title', content: pageTitle },
            { name: 'twitter:description', content: description },
            { name: 'twitter:image', content: image },
        ],
        link: [
            { rel: 'canonical', href: canonical }
        ]
    })
}
