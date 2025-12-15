const target = 'http://localhost:5283'
const isProd = process.env.NODE_ENV === 'production';



export default defineNuxtConfig({
    modules: ['notivue/nuxt'],

    css: [
        'notivue/notification.css',
        'notivue/animations.css'
    ],

    notivue: {
        position: 'bottom-right',
        //limit: 4,
        enqueue: true,
        avoidDuplicates: true,
        notifications: {
            global: {
                duration: 6000
            }
        }
    },

    compatibilityDate: '2025-07-15',
    devtools: {
      enabled: true,

      timeline: {
        enabled: true
      }
    },

    vite: {
        css: {
            modules: {
                generateScopedName: isProd ? '[hash:base64:11]' : '[local]__[hash:base64:6]',
            }
        }
    },

    devServer: {
        //host: "0.0.0.0",
        port: 3226,
    },

    nitro: {
        routeRules: {
            '/api/**': {
                proxy: `${target}/api/**`
            },
        },

        devProxy: {
            '/_openapi/': { target: `${target}/_openapi/`, changeOrigin: true },
            '/_swagger/': { target: `${target}/_swagger/`, changeOrigin: true },
        },
    },

    runtimeConfig: {
        //baseUrlInternal: "http://172.17.0.1:8350",
    },

    experimental: {
        viewTransition: true,
        //payloadExtraction: true,
    },
})