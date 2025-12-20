// https://nuxt.com/docs/api/configuration/nuxt-config

import { fileURLToPath } from 'url'

const isProd = process.env.NODE_ENV === 'production';

const target = process.env.ASPNETCORE_HTTPS_PORT
    ? `http://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
    : process.env.ASPNETCORE_URLS
        ? process.env.ASPNETCORE_URLS.split(';')[0]
        : 'http://localhost:5283'

export default defineNuxtConfig({
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

            '/**': {
                prerender: false
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