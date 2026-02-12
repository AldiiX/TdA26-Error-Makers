import crypto  from 'node:crypto'
import path from "node:path";

const target = 'http://localhost:5283'
const isProd = process.env.NODE_ENV === 'production';

function stableHash(input: string, length: number = 11): string {
    return crypto.createHash("sha256").update(input).digest("base64url").slice(0, length);
}

function stablePrefixLetter(input: string): string {
    const alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const digest = crypto.createHash("sha256").update(input).digest();
    return alphabet[digest[0]! % alphabet.length]!;
}

export default defineNuxtConfig({
    modules: ['notivue/nuxt'],

    css: [
        '@/assets/global.scss',
        'notivue/notification.css',
        'notivue/animations.css',
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
                generateScopedName(className: string, fileName: string) {
                    const cleanFileName = (fileName.split("?")[0] ?? fileName);

                    const normalizedFileName = cleanFileName.replaceAll("\\", "/");
                    const normalizedRoot = process.cwd().replaceAll("\\", "/");

                    const relativeFile = path.posix.relative(normalizedRoot, normalizedFileName);

                    const seed = `${relativeFile}:${className}`;
                    const hash = stableHash(seed);
                    const prefix = stablePrefixLetter(seed);

                    return isProd ? `${prefix}${hash}` : `${className}__${prefix}${hash}`;
                },
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

    features: {
        inlineStyles: true,
    },
})